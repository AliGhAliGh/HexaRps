using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Utilities.Data;
using Utilities.Ui;

namespace Menu
{
	public class MenuManager : Singleton<MenuManager>
	{
		[SerializeField] private CustomTextButton mainButton;
		[SerializeField] private CustomField mainField;
		[SerializeField] private GameObject a;

		private async void Start()
		{
			NetworkManager.OnGameStart = () =>
			{
				LogManager.ShowWarning("start game");
				if (SceneManager.GetActiveScene().buildIndex == 0) SceneManager.LoadScene(1);
			};
			await NetworkManager.Connect();

			if (SceneManager.GetActiveScene().buildIndex == 1) return;

			if (NetworkManager.IsLoggedIn)
				Success();
			else
			{
				mainField.Select();
				mainField.Text = "";
				mainField.Placeholder = "Email";
				mainButton.Interactable = true;
				mainButton.text.SetText("Login");
				mainField.OnSubmit = mainButton.OnClick = CheckName;
			}

			return;

			async void CheckName()
			{
				var email = mainField.Text;
				if (email.Length < 6)
					return;
				mainButton.Interactable = mainField.Interactable = false;
				var exist = await NetworkManager.EmailExist(email);
				LogManager.ShowMessage("check name: " + exist);
				mainField.Text = "";
				mainField.Placeholder = "Password";
				CoroutineRunner.DelayedRun(mainField.Select);
				mainField.Interactable = mainButton.Interactable = true;
				mainButton.text.SetText(exist ? "Login" : "Signup");
				mainField.OnSubmit = mainButton.OnClick = exist ? Login : Signup;
				return;

				async void Login()
				{
					var password = mainField.Text;
					if (await NetworkManager.Login(email, password))
						Success();
				}

				async void Signup()
				{
					var password = mainField.Text;
					if (await NetworkManager.Signup(email, password))
						Success();
				}
			}

			void Success()
			{
				mainField.gameObject.SetActive(false);
				mainButton.gameObject.SetActive(false);
				mainButton.text.SetText("StartMatch");
				LogManager.ShowMessage(Color.green, "success!");
				MatchManager.Show();
			}
		}

		private string _matchId;

		private async void Update()
		{
			if (!Input.GetKey(KeyCode.LeftShift)) return;

			if (Input.GetKeyDown(KeyCode.H))
				_matchId = (string)FlexData.Parse(await NetworkManager.RPC("CreateMatch", new FlexData
				{
					["matchName"] = "aligh"
				}.ToJson))["matchId"];

			if (Input.GetKeyDown(KeyCode.G))
			{
				await NetworkManager.JoinMatch(_matchId);
			}

			if (Input.GetKeyDown(KeyCode.J))
				PlayerPrefs.DeleteAll();
		}
	}
}
