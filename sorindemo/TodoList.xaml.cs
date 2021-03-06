﻿#define AUTH_ENABLED
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace sorindemo
{
    public partial class TodoList : ContentPage
    {
        TodoItemManager manager;
        bool IsAuthenticated { get { return TodoItemManager.DefaultManager.CurrentClient.CurrentUser != null; } }

        public TodoList()
        {
            InitializeComponent();

            manager = TodoItemManager.DefaultManager;

//#if AUTH_ENABLED
//            var loginButton = new Button
//            {
//                Text = "Login",
//                TextColor = Xamarin.Forms.Color.Black,
//                BackgroundColor = Xamarin.Forms.Color.Lime,
//            };
//            loginButton.Clicked += loginButton_Clicked;

//            Xamarin.Forms.StackLayout bp = buttonsPanel as StackLayout;
//            Xamarin.Forms.StackLayout bpParentStack = bp.Parent.Parent as StackLayout;

//            bpParentStack.Padding = new Xamarin.Forms.Thickness(10, 30, 10, 20);
//            bp.Orientation = StackOrientation.Vertical;
//            bp.Children.Add(loginButton);
//#endif

            // OnPlatform<T> doesn't currently support the "Windows" target platform, so we have this check here.
            if (manager.IsOfflineEnabled &&
                (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone))
            {
                //syncButton.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Set syncItems to true in order to synchronize the data on startup when running in offline mode
#if AUTH_ENABLED
            //if (IsAuthenticated)
#endif
            await RefreshItems(true, syncItems: false);
        }

        // Data methods
        async Task AddItem(TodoItem item)
        {
            try
            {
                await manager.SaveTaskAsync(item);
                todoList.ItemsSource = (await manager.GetTodoItemsAsync()).Items;
            }
            catch (Exception ex)
            {
                await DisplayAlert(item.Name, "Could not add task " + item.Name + ", check your network connection!", "Close");
                Debug.WriteLine(ex);
            }
        }

        async Task CompleteItem(TodoItem item)
        {
            try
            {
                item.Done = true;
                await manager.SaveTaskAsync(item);
                todoList.ItemsSource = (await manager.GetTodoItemsAsync()).Items;
            }
            catch (Exception ex)
            {
                await DisplayAlert(item.Name, "Could not complete task " + item.Name + ", check your network connection!", "Close");
                Debug.WriteLine(ex);
            }
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(newItemName.Text))
                return;
            var todo = new TodoItem { Name = newItemName.Text };
            await AddItem(todo);

            newItemName.Text = string.Empty;
            newItemName.Unfocus();
        }

        // Event handlers
        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var todo = e.SelectedItem as TodoItem;
            if (Device.OS != TargetPlatform.iOS && todo != null)
            {
                // Not iOS - the swipe-to-delete is discoverable there
                if (Device.OS == TargetPlatform.Android)
                {
                    await DisplayAlert(todo.Name, "Press-and-hold to complete task " + todo.Name, "Got it!");
                }
                else
                {
                    // Windows, not all platforms support the Context Actions yet
                    if (await DisplayAlert("Mark completed?", "Do you wish to complete " + todo.Name + "?", "Complete", "Cancel"))
                    {
                        await CompleteItem(todo);
                    }
                }
            }

            // prevents background getting highlighted
            todoList.SelectedItem = null;
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#context
        public async void OnComplete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var todo = mi.CommandParameter as TodoItem;
            await CompleteItem(todo);
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#pulltorefresh
        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            Exception error = null;
            try
            {
                await RefreshItems(false, true);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                list.EndRefresh();
            }

            if (error != null)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + error.Message + ")", "OK");
            }
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await RefreshItems(true, true);
        }

        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            if (!Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Error on Refresh", "Could not refresh data - check your network connection.", "OK");
                return;
            }

            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                var result = await manager.GetTodoItemsAsync(syncItems);
                if (result == null)
                    await DisplayAlert("Error on Refresh", "Could not refresh data - check your network connection.", "OK");
                else
                {
                    todoList.ItemsSource = result.Items;
                    if (result.Errors != null)
                    {
                        foreach (var error in result.Errors)
                        {
                            if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                            {
                                await DisplayAlert("Sync Error", $"{error.Item["text"]}: this item has already been completed by someone else.", "OK");
                            }
                        }
                    }
                }
            }
        }

        //private async Task ResolveConflict(TodoItem localItem, TodoItem serverItem)
        //{
        //    //Ask user to choose the resolution between versions
        //    MessageDialog msgDialog = new MessageDialog(String.Format("Server Name: \"{0}\" \nLocal Name: \"{1}\"\n",
        //                                                serverItem.Name, localItem.Name),
        //                                                "CONFLICT DETECTED - Select a resolution:");
        //    UICommand localBtn = new UICommand("Commit Local Text");
        //    UICommand ServerBtn = new UICommand("Leave Server Text");
        //    msgDialog.Commands.Add(localBtn);
        //    msgDialog.Commands.Add(ServerBtn);
        //    //localBtn.Invoked = async (IUICommand command) =>
        //    //{
        //    //    // To resolve the conflict, update the version of the
        //    //    // item being committed. Otherwise, you will keep
        //    //    // catching a MobileServicePreConditionFailedException.
        //    //    //localItem.Version = serverItem.Version;
        //    //    // Updating recursively here just in case another
        //    //    // change happened while the user was making a decision
        //    //    //await UpdateToDoItem(localItem);
        //    //};
        //    //ServerBtn.Invoked = async (IUICommand command) =>
        //    //{
        //    //    //RefreshTodoItems();
        //    //};
        //    await msgDialog.ShowAsync();
        //}

#if AUTH_ENABLED
        async void OnLogin(object sender, EventArgs e)
        {
            try
            {
                var providerString = await DisplayActionSheet("Login With", "Cancel", null, "Facebook", "Google", "Microsoft Account", "Twitter", "Azure AD");
                var provider = MobileServiceAuthenticationProvider.MicrosoftAccount;

                switch (providerString)
                {
                    case "Facebook": provider = MobileServiceAuthenticationProvider.Facebook; break;
                    case "Google": provider = MobileServiceAuthenticationProvider.Google; break;
                    case "Microsoft Account": provider = MobileServiceAuthenticationProvider.MicrosoftAccount; break;
                    case "Twitter": provider = MobileServiceAuthenticationProvider.Twitter; break;
                    case "Azure AD": provider = MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory; break;
                    default:
                        return;
                }
                if (App.Authenticator != null)
                    await App.Authenticator.Authenticate(provider);

                // Set syncItems to true in order to synchronize the data on startup when running in offline mode
                if (IsAuthenticated)
                {
                    var userdata = await TodoItemManager.DefaultManager.CurrentClient.InvokeApiAsync("userdata", HttpMethod.Get ,null);
                    var user = User.GetUserInfo(userdata, provider);
                    await DisplayAlert("Authenticated", $"{user.FullName} is logged in.", "OK");
                    labelUser.Text = user.FullName;
                    //login.IsVisible = false;
                    await RefreshItems(true, syncItems: false);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Authentication failed", ex.Message, "OK");
            }
        }
#endif

        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}

