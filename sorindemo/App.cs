using Microsoft.WindowsAzure.MobileServices;
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
			MainPage = new TodoList();
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
    }
}

