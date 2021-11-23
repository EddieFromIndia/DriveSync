namespace DriveSync.Services;

public class DialogService
{
    public static DialogResult ShowDialog(string title, string body, DialogButtonGroup buttons, DialogImage image)
    {
        DialogWindow dialogWindow = new(title, body, buttons, image);

        _ = dialogWindow.ShowDialog();

        return dialogWindow.ButtonClicked;
    }
}
