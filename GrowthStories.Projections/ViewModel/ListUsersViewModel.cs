﻿using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using Growthstories.Core;
using EventStore.Logging;
using CommonDomain;

namespace Growthstories.UI.ViewModel
{

    //public sealed class RemoteUser
    //{
    //    public readonly string Name;


    //    public RemoteUser(string name)
    //    {
    //        this.Name = name;
    //    }
    //}

    public sealed class ListUsersViewModel : RoutableViewModel, IControlsAppBar, IControlsProgressIndicator, IControlsSystemTray
    {
        private readonly ITransportEvents Transporter;

        public readonly IObservable<IUserListResponse> SearchResults;
        public readonly IObservable<List<CreateSyncStream>> SyncStreams;

        private static ILog Logger = LogFactory.BuildLogger(typeof(ListUsersViewModel));


        public ReactiveList<RemoteUser> List { get; private set; }
        public ReactiveCommand SearchCommand { get; private set; }
        public ReactiveCommand UserSelectedCommand { get; private set; }

        protected bool _InProgress;
        public bool ProgressIndicatorIsVisible
        {
            get
            {
                return _InProgress;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _InProgress, value);
            }
        }


        public ListUsersViewModel(ITransportEvents transporter, IGSAppViewModel app)
            : base(app)
        {

            Transporter = transporter;
            List = new ReactiveList<RemoteUser>();
            SearchCommand = new ReactiveCommand();
            ProgressIndicatorIsVisible = false;

            var input = SearchCommand
                .OfType<string>()
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 2)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .DistinctUntilChanged();



            var results = input
                .Select(s =>
                {
                    return transporter.ListUsersAsync(s).ToObservable();
                })
                .Merge()
                .ObserveOn(RxApp.MainThreadScheduler);


            input.Subscribe(_ => ProgressIndicatorIsVisible = true);
            results.Subscribe(x =>
            {
                ProgressIndicatorIsVisible = false;
                List.Clear();
                if (x.Users != null && x.Users.Count > 0)
                {

                    List.AddRange(x.Users);
                }
            });

            this.SearchResults = results;

            UserSelectedCommand = new ReactiveCommand();
            //UserSelectedCommand.Re

            UserSelectedCommand.Subscribe(_ => ProgressIndicatorIsVisible = true);

            this.SyncStreams = UserSelectedCommand
                .RegisterAsyncTask<List<CreateSyncStream>>(y =>
                    Task.Run(async () =>
                    {
                        var x = y as RemoteUser;
                        if (x == null)
                            return null;
                        var cmds = new List<CreateSyncStream>() 
                            {
                                new CreateSyncStream(x.AggregateId, Core.StreamType.USER)
                            };
                        if (x.Garden != null && x.Garden.Plants != null)
                        {
                            foreach (var p in x.Garden.Plants)
                                cmds.Add(new CreateSyncStream(p.AggregateId, Core.StreamType.PLANT, x.AggregateId));

                        }

                        foreach (var cmd in cmds)
                        {
                            try
                            {

                                app.Model.Handle(cmd);
                            }
                            catch (DomainError)
                            {
                                // we already have the stream as a syncstream
                            }
                        }
                        if (!app.Model.HasUncommittedEvents)
                            return null;

                        //Repository.Save(app.Model);

                        //await App.Synchronize();
                        //App.Router.NavigateBack.Execute(null);
                        return cmds;
                    })
                );


            this.SyncStreams
                .Subscribe(x =>
                {
                    ProgressIndicatorIsVisible = false;
                    App.Router.NavigateBack.Execute(null);
                });



        }




        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return false; }
        }


        public bool SystemTrayIsVisible
        {
            get { return true; }
        }
    }


    public sealed class ListUsersViewModelDesign
    {

        public IList<RemoteUser> List { get; private set; }
        public bool InProgress { get; private set; }
        public ListUsersViewModelDesign()
        {
            List = new List<RemoteUser>() 
            {
                new RemoteUser()
                {
                    Username = "Lauri"
                },
                new RemoteUser()
                {
                    Username = "Jonathan"
                }
            };

        }


    }



}
