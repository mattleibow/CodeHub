﻿using CodeHub.Helpers;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeHub.Services;
using System.Collections.ObjectModel;
using Octokit;

namespace CodeHub.ViewModels
{
    public class NotificationsViewmodel : AppViewmodel
    {
        public ObservableCollection<Notification> _Notifications;
        public ObservableCollection<Notification> Notifications
        {
            get
            {
                return _Notifications;
            }
            set
            {
                Set(() => Notifications, ref _Notifications, value);
            }
        }

        public bool _ZeroNotificationCount;
        public bool ZeroNotificationCount
        {
            get
            {
                return _ZeroNotificationCount;
            }
            set
            {
                Set(() => ZeroNotificationCount, ref _ZeroNotificationCount, value);
            }
        }

        public RelayCommand _loadCommand;
        public RelayCommand LoadCommand
        {
            get
            {
                return _loadCommand
                    ?? (_loadCommand = new RelayCommand(
                                          async () =>
                                          {

                                              if (!GlobalHelper.IsInternet())
                                              {
                                                  //Sending NoInternet message to all viewModels
                                                  Messenger.Default.Send(new GlobalHelper.LocalNotificationMessageType { Message = "No Internet", Glyph = "\uE704" });
                                              }
                                              else
                                              {
                                                  if(Notifications == null)
                                                  {   
                                                      isLoading = true;
                                                      await LoadAllNotifications();
                                                      isLoading = false;
                                                  }
                                                  else
                                                  {
                                                      /*Silent loading */
                                                      await LoadAllNotifications();
                                                  }


                                              }
                                          }));
            }
        }


        public RelayCommand<Notification> _MarkasReadCommand;
        public RelayCommand<Notification> MarkasReadCommand
        {
            get
            {
                return _MarkasReadCommand
                    ?? (_MarkasReadCommand = new RelayCommand<Notification>(
                                          async (Notification notification) =>
                                          {
                                              await NotificationsService.MarkNotificationAsRead(notification.Id);
                                          }));
            }
        }
        public RelayCommand<Notification> _UnsubscribeCommand;
        public RelayCommand<Notification> UnsubscribeCommand
        {
            get
            {
                return _UnsubscribeCommand
                    ?? (_UnsubscribeCommand = new RelayCommand<Notification>(
                                          async (Notification notification) =>
                                          {
                                              await NotificationsService.UnsubscribeFromThread(notification.Id);
                                          }));
            }
        }

        //public async void RefreshCommand(object sender, EventArgs e)
        //{

        //    if (!GlobalHelper.IsInternet())
        //    {
        //        //Sending NoInternet message to all viewModels
        //        Messenger.Default.Send(new GlobalHelper.LocalNotificationMessageType { Message = "No Internet", Glyph = "\uE704" });
        //    }
        //    else
        //    {

        //        Messenger.Default.Send(new GlobalHelper.HasInternetMessageType()); //Sending Internet available message to all viewModels
        //        isLoading = true;
        //        await LoadAllNotifications();
        //        isLoading = false;
        //    }
        //}

        public void RecieveSignOutMessage(GlobalHelper.SignOutMessageType empty)
        {
            isLoggedin = false;
            User = null;
            Notifications = null;
        }
        public async void RecieveSignInMessage(User user)
        {

            isLoading = true;
            if (user != null)
            {
                isLoggedin = true;
                User = user;
                await LoadAllNotifications();
            }
            isLoading = false;

        }
        private async Task LoadAllNotifications()
        {
            Notifications = await NotificationsService.GetAllNotificationsForCurrentUser(true,false);
            if (Notifications != null)
            {
                ZeroNotificationCount = (Notifications.Count == 0) ? true : false;
            }
        }
        private async Task LoadUnreadNotifications()
        {
            Notifications = await NotificationsService.GetAllNotificationsForCurrentUser(false, false);
            if (Notifications != null)
            {
                ZeroNotificationCount = (Notifications.Count == 0) ? true : false;
            }
        }
        private async Task LoadParticipatingNotifications()
        {
            Notifications = await NotificationsService.GetAllNotificationsForCurrentUser(true, true);
            if (Notifications != null)
            {
                ZeroNotificationCount = (Notifications.Count == 0) ? true : false;
            }
        }

    }
}
