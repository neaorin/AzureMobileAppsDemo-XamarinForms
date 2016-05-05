using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace sorindemo
{
	public class App : Application
	{
        public static IAuthenticate Authenticator { get; private set; }

        public static void Init(IAuthenticate authenticator)
        {
            Authenticator = authenticator;
        }

        public App ()
		{
			// The root page of your application
			MainPage = new NavigationPage(new TodoList());
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}

    public interface IAuthenticate
    {
        Task<MobileServiceUser> Authenticate(MobileServiceAuthenticationProvider provider);
    };

    public interface IDisplayAlert
    {
        void DisplayAlert(string title, string message);
    }

    public class User
    {
        public string FullName { get; set; }

        public static User GetUserInfo(JToken userdata, MobileServiceAuthenticationProvider provider)
        {
            var user = new User();

            switch (provider)
            {
                case MobileServiceAuthenticationProvider.Facebook: user.FullName = userdata["facebook"]["user_id"].ToString(); break;
                case MobileServiceAuthenticationProvider.Google: user.FullName = userdata["google"]["user_id"].ToString(); break;
                case MobileServiceAuthenticationProvider.MicrosoftAccount: user.FullName = userdata["microsoftaccount"]["user_id"].ToString(); break;
                case MobileServiceAuthenticationProvider.Twitter: user.FullName = userdata["twitter"]["user_id"].ToString(); break;
                case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory: user.FullName = $"{userdata["aad"]["claims"]["givenname"].ToString()} {userdata["aad"]["claims"]["surname"].ToString()}"; break;
            }
            return user;
        }
    }
}

