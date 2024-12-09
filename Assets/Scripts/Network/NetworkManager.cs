using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utilities;
using Utilities.Data;
using Random = System.Random;

namespace Network
{
	public static class NetworkManager
	{
		private const string IP = "193.3.231.238";
		// private const string IP = "localhost";
		private const string DEFAULT_EMAIL = "DefaultEmail";
		public const int LOBBY_SIZE = 2;

		private static IClient _client;
		private static ISession _session;
		private static ISocket _socket;
		private static string _matchTicket;
		private static string _matchId;
		private static Random _random;

		public static Action OnGameStart;
		private static readonly List<User> Presences = new();

		private static readonly RetryConfiguration RetryConfiguration = new(1, 5, (a, _) =>
			LogManager.ShowMessage($"{a}:retrying..."), RetryJitter.FullJitter);

		public static bool IsLoading { get; private set; }

		public static bool IsLoggedIn => _session != null;
		public static string OwnerID => _session?.UserId;
		private static User OwnerUser => Presences.FirstOrDefault(c => c.UserId == OwnerID);

		#region DB

		private static async Task<FlexData> Load(string collection, string key)
		{
			var readObjectId = new StorageObjectId
			{
				Collection = collection,
				Key = key,
				UserId = _session.UserId
			};
			var result =
				await _client.ReadStorageObjectsAsync(_session, new IApiReadStorageObjectId[] { readObjectId });

			var res = result.Objects.FirstOrDefault()?.Value;

			return res == null ? null : (FlexData)JObject.Parse(res);
		}

		public static async Task<IEnumerable<FlexData>> GetList(string collection)
		{
			var res = new List<IApiStorageObject>();
			const int limit = 10; // default is 1.
			var result = await _client.ListUsersStorageObjectsAsync(_session, collection, _session.UserId, limit);
			res.AddRange(result.Objects);

			while (!string.IsNullOrEmpty(result.Cursor))
			{
				result = await _client.ListUsersStorageObjectsAsync(_session, collection, _session.UserId, limit,
					result.Cursor);
				res.AddRange(result.Objects);
			}

			return res.Select(c => new FlexData(JObject.Parse(c.Value)));
		}

		private static async Task Save(string collection, string key, FlexData data, bool isPublic)
		{
			try
			{
				var storageObject = new WriteStorageObject
				{
					Collection = collection,
					Key = key,
					Value = data.ToJson,
					PermissionRead = isPublic ? 2 : 1,
					PermissionWrite = 1
				};

				await _client.WriteStorageObjectsAsync(_session,
					new IApiWriteStorageObject[] { storageObject });
			}
			catch (Exception ex)
			{
				LogManager.ShowMessage(Color.red, ex.Message);
			}
		}

		private static async Task Delete(string collection, string key) =>
			await _client.DeleteStorageObjectsAsync(_session, new[]
			{
				new StorageObjectId
				{
					Collection = collection,
					Key = key,
					UserId = _session.UserId
				}
			});

		#endregion

		#region UserManager

		public static int GameId => OwnerUser.Index;

		public static string GetUserByIndex(int index) => Presences.FirstOrDefault(c => c.Index == index)?.UserId;

		private static void SortUsers()
		{
			Presences.Sort((c1, c2) => string.Compare(c2.UserId, c1.UserId, StringComparison.Ordinal));
			// Presences.SyncShuffle();
			for (var i = 0; i < Presences.Count; i++) Presences[i].SetId(i);
		}

		#endregion

		#region Random

		public static int SyncRandom(int a, int b) => _random.Next(a, b);

		public static List<T> SyncPick<T>(int count, params T[] data)
		{
			var res = new List<T>();
			var list = data.ToList();
			for (var i = 0; i < count; i++)
			{
				var index = SyncRandom(0, list.Count);
				res.Add(list[index]);
				list.RemoveAt(index);
			}

			return res;
		}

		public static bool SyncRandom() => _random.Next() % 2 == 0;

		public static T SyncRandomItem<T>(this List<T> data) => data[SyncRandom(0, data.Count)];

		public static void SyncShuffle<T>(this List<T> data)
		{
			var copy = data.ToList();
			data.Clear();
			while (copy.Count > 0)
			{
				var r = copy.SyncRandomItem();
				copy.Remove(r);
				data.Add(r);
			}
		}

		#endregion

		#region Connection

		public static async Task Connect()
		{
			Init();
			await TryLogin(DataManager.GetPrefs(DEFAULT_EMAIL));
		}

		public static async Task<bool> EmailExist(string email)
		{
			try
			{
				await _client.AuthenticateEmailAsync(email, "````````", create: false);
				return false;
			}
			catch (ApiResponseException e)
			{
				LogManager.ShowError(e.Message);
				return e.Message == "Invalid credentials.";
			}
		}

		public static async Task<bool> Login(string email, string password)
		{
			await RecreateSession(email, password);
			if (!IsLoggedIn) return false;
			DataManager.SetPrefs(DEFAULT_EMAIL, email);
			while (!await SetSocket())
				await RecreateSession(email, password);
			return true;
		}

		public static async Task<bool> Signup(string email, string password)
		{
			_session = await GetSession(email, password, true);
			if (!IsLoggedIn) return false;
			DataManager.SaveData(new LoginData(_session.AuthToken, _session.RefreshToken, email, password), email);
			DataManager.SetPrefs(DEFAULT_EMAIL, email);
			while (!await SetSocket())
				await RecreateSession(email, password);
			return true;
		}

		private static async Task TryLogin(string email)
		{
			var data = DataManager.LoadData<LoginData>(email);

			if (data == null || string.IsNullOrEmpty(data.AuthToken)) return;

			await Login(data);

			while (!await SetSocket())
				await RecreateSession(data.Email, data.Password);

			await CheckLastMatch();
		}

		private static void Init()
		{
			Application.targetFrameRate = 60;
			Application.runInBackground = true;
			_session = null;
			_matchId = null;
			_client = new Client("http", IP, 7350, "AliGh", UnityWebRequestAdapter.Instance, false);
			_client.Timeout = 5;
			_client.GlobalRetryConfiguration = RetryConfiguration;
		}

		private static async Task Login(LoginData data)
		{
			var session = Session.Restore(data.AuthToken, data.RefreshToken);
			if (!session.HasExpired(DateTime.UtcNow.AddMinutes(20)))
			{
				_session = session;
			}
			else
			{
				try
				{
					_session = await _client.SessionRefreshAsync(session);
					DataManager.SaveData(
						data with { AuthToken = _session.AuthToken, RefreshToken = _session.RefreshToken }, data.Email);
				}
				catch (ApiResponseException)
				{
					await RecreateSession(data.Email, data.Password);
				}
			}
		}

		private static async Task<ISession> GetSession(string email, string password, bool create = false)
		{
			try
			{
				return await _client.AuthenticateEmailAsync(email, password, email, create);
			}
			catch (ApiResponseException e)
			{
				LogManager.ShowError(e.Message);
				return null;
			}
		}

		private static async Task RecreateSession(string email, string password)
		{
			_session = await GetSession(email, password);
			if (_session == null) return;
			DataManager.SaveData(new LoginData(_session.AuthToken, _session.RefreshToken, email, password), email);
		}

		private static async Task<bool> SetSocket()
		{
			_socket = _client.NewSocket(true, new WebSocketAdapter());

			var success = await ConnectSocket();
			if (!success)
				return false;

			_socket.Closed += () => LogManager.ShowMessage("disconnected!");
			_socket.ReceivedMatchState += c => MainThread.Enqueue(() =>
			{
				switch (c.OpCode)
				{
					case 1:
						var playersMessage = Encoding.UTF8.GetString(c.State);
						Presences.Clear();
						foreach (var userId in playersMessage.Split('|')) Presences.Add(new(userId));
						SortUsers();
						OnGameStart?.Invoke();
						return;
					case 2:
						var syncMessage = Encoding.UTF8.GetString(c.State);
						var split = syncMessage.Split('|');
						SyncMessages.AddRange(split.Select(SyncMessage.Parse));
						return;
					default:
						OnReceived(c);
						break;
				}
			});
			_socket.ReceivedNotification += notification =>
			{
				LogManager.ShowMessage($"id: {notification.Id}");
				LogManager.ShowMessage($"Code: {notification.Code}");
				LogManager.ShowMessage($"Persistent: {notification.Persistent}");
				LogManager.ShowMessage($"Subject: {notification.Subject}");
				LogManager.ShowMessage($"content: '{notification.Content}'");
			};
			return true;

			async Task<bool> ConnectSocket()
			{
				try
				{
					if (!_socket.IsConnected)
						await _socket.ConnectAsync(_session, true);
					return true;
				}
				catch (Exception e)
				{
					LogManager.ShowError("Error connecting socket: " + e.Message);
					LogManager.ShowWarning("trying to recreate session!");
					return false;
				}
			}
		}

		private static async Task CheckLastMatch()
		{
			var matchData = await Load("Match", "LastMatch");

			try
			{
				if (matchData == null) return;
				await JoinMatch(matchData["matchId"].GetData(""));
			}
			catch
			{
				await Delete("Match", "LastMatch");
			}
		}

		#endregion

		#region RPC

		public static async Task<string> RPC(string name, string payLoad)
		{
			var res = await _socket.RpcAsync(name, payLoad);
			return res.Payload;
		}

		public static async Task<IApiAccount> GetAccount() => await _client.GetAccountAsync(_session);

		public static async Task UpdateAccount(string displayName) =>
			await _client.UpdateAccountAsync(_session, _session.Username, displayName);

		#endregion

		#region Matchmake

		public static bool IsMyMatch(string matchId) => _matchId == matchId;

		public static async Task CreateServerMatch(string matchName)
		{
			await JoinMatch((string)FlexData.Parse(await RPC("CreateMatch", new FlexData
			{
				["matchName"] = matchName
			}.ToJson))["matchId"]);
		}

		public static async Task<IEnumerable<MatchResult>> GetMatches()
		{
			var matchList = (await RPC("MatchList", ""))?.Split('|');
			if (matchList == null)
				return new List<MatchResult>();

			var res = new List<MatchResult>();
			foreach (var match in matchList)
			{
				var flex = FlexData.Parse(match);
				var label = FlexData.Parse((string)flex["label"]);
				var name = (string)label["name"];
				var ownerId = (string)label["ownerId"];
				res.Add(new MatchResult((string)flex["matchId"], (int)flex["size"], name, ownerId));
			}

			return res;
		}

		public static async Task JoinMatch(string matchId)
		{
			var match = await _socket.JoinMatchAsync(matchId);
			await SetMatchData(match);
		}

		public static async Task Quit()
		{
			if (_matchId != null)
				await _socket.LeaveMatchAsync(_matchId);
		}

		private static async Task SetMatchData(IMatch match)
		{
			IsLoading = true;
			_matchId = match.Id;
			_random = new Random(match.Id.GetHashCode());

			var data = new FlexData
			{
				["matchId"] = match.Id
			};
			await Save("Match", "LastMatch", data, false);
		}

		#endregion

		#region Sync

		private static readonly List<SyncMessage> SyncMessages = new();

		public static void ConsumeSyncMessages()
		{
			SyncMessages.ForEach(OnReceived);
			SyncMessages.Clear();
			CoroutineRunner.WaitRun(.2f, () => IsLoading = false);
		}

		#endregion

		#region BroadCaster

		private static readonly Dictionary<OpCode, Action<string, ByteArray, bool>> Receivers = new();

		public static void Broadcast(OpCode opCode, params object[] data) =>
			_socket.SendMatchStateAsync(_matchId, (long)opCode, new ByteArray(data));

		private static void OnReceived(IMatchState state)
		{
			if (Receivers.TryGetValue((OpCode)state.OpCode, out var action))
				action?.Invoke(state.UserPresence.UserId, new(state.State), false);
			else LogManager.ShowError("no receiver: " + (OpCode)state.OpCode);
		}

		private static void OnReceived(SyncMessage syncMessage)
		{
			if (Receivers.TryGetValue(syncMessage.OpCode, out var action))
				action?.Invoke(syncMessage.SenderId, syncMessage.Data, true);
			else LogManager.ShowError("no receiver: " + syncMessage.OpCode);
		}

		public static void AddBroadcastReceiver(OpCode opCode, Action<string, ByteArray, bool> action) =>
			Receivers[opCode] = action;

		#endregion
	}

	public enum OpCode
	{
		AddBlock = 13
	}

	public class User : IEquatable<User>
	{
		public readonly string UserId;
		public int Index { get; private set; }

		public User(string userId) => (UserId, Index) = (userId, -1);

		public void SetId(int id) => Index = id;

		public override bool Equals(object obj) => obj is User user && UserId.Equals(user.UserId);

		public bool Equals(User other)
		{
			if (ReferenceEquals(null, other)) return false;
			return ReferenceEquals(this, other) || Equals(UserId, other.UserId);
		}

		public override int GetHashCode() => UserId != null ? UserId.GetHashCode() : 0;

		public static bool operator ==(User left, User right) => Equals(left, right);

		public static bool operator !=(User left, User right) => !Equals(left, right);
	}

	public class MatchResult
	{
		private readonly string _ownerId;

		public string ID { get; }
		public int Size { get; }
		public string Name { get; }

		public bool IsOwner => NetworkManager.OwnerID == _ownerId;
		public bool IsJoined => NetworkManager.IsMyMatch(ID);

		public MatchResult(string id, int size, string name, string ownerId)
		{
			ID = id;
			Size = size;
			Name = name;
			_ownerId = ownerId;
		}
	}

	public class SyncMessage
	{
		public readonly string SenderId;
		public readonly OpCode OpCode;
		public readonly ByteArray Data;

		private SyncMessage(string senderId, OpCode opCode, ByteArray data)
		{
			SenderId = senderId;
			OpCode = opCode;
			Data = data;
		}

		public static SyncMessage Parse(string message)
		{
			var obj = JObject.Parse(message);
			var sender = (string)obj["sender"];
			var data = Convert.FromBase64String((string)obj["data"] ?? string.Empty);
			var opCode = (OpCode)(int)obj["opCode"];
			return new(sender, opCode, data);
		}
	}
}
