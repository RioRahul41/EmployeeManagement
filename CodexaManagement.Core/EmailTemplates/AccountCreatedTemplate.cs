
using System.Net;

namespace CodexaManagement.Core.EmailTemplates;

public static class AccountCreatedTemplate
{
    public static string GetHtml(
        string firstName,
        string email,
        string temporaryPassword,
        string loginUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;
                         background-color:#f4f6f9;padding:24px;">
                <div style="max-width:600px;margin:auto;
                            background:#ffffff;padding:30px;
                            border-radius:10px;">
                    <h2 style="color:#2457a7;">
                        Welcome to CodexaManagement!
                    </h2>

                    <p>Hello {WebUtility.HtmlEncode(firstName)},</p>

                    <p>Your account has been created successfully.</p>

                    <p><strong>Email:</strong>
                       {WebUtility.HtmlEncode(email)}</p>

                    <p><strong>Temporary password:</strong>
                       {WebUtility.HtmlEncode(temporaryPassword)}</p>

                    <p>Please log in and change your temporary
                       password immediately.</p>

                    <a href="{WebUtility.HtmlEncode(loginUrl)}"
                       style="background:#2457a7;color:#ffffff;
                              padding:12px 20px;text-decoration:none;
                              border-radius:5px;display:inline-block;">
                        Login to your account
                    </a>

                    <p style="margin-top:25px;color:#777;">
                        If you did not expect this account,
                        please contact your administrator.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
