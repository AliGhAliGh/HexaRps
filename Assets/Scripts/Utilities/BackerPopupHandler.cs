namespace Utilities
{
	public class BackerPopupHandler : PopupHandler, IBack
	{
		public override void Open() => Open(this);

		public override void Close() => Back();

		public void Back() => Close(this);
	}
}
