using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Common;
using Android.Gms.AppStates;
using Android.Gms.Games;
using Android.Gms.Games.MultiPlayer;
using Android.Gms.Games.MultiPlayer.RealTime;
using Android.Gms.Games.MultiPlayer.TurnBased;
using System.Collections.Generic;

namespace CoponentTest
{
    [Activity(Label = "CoponentTest", MainLauncher = true)]
    public class MainActivity : Activity
    , IGoogleApiClientConnectionCallbacks
    , IGoogleApiClientOnConnectionFailedListener
    , IResultCallback 
    , IOnTurnBasedMatchUpdateReceivedListener
    {
        public static int REQUEST_CODE_RESOLVE_ERR = 9000;
        public static int RC_SELECT_PLAYERS = 9001;
        public static int RC_OPEN_INBOX = 9002;

        ITurnBasedMatch mMatch;

        int count = 0;

        private IGoogleApiClient mGoogleApiClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //mGoogleApiClient = new GoogleApiClientBuilder(this).AddApi(Plus.Api).AddScope(new Scope(Scopes.Games)).Build();

            // create an instance of Google API client and specify the Play services 
            // and scopes to use. In this example, we specify that the app wants 
            // access to the Games, Plus, and Cloud Save services and scopes.
            GoogleApiClientBuilder builder = 
                new GoogleApiClientBuilder(this, this, this);

            builder.AddApi(GamesClass.Api).AddScope(GamesClass.ScopeGames);
            /*
            builder.AddApi(GamesClass.Api)
                .AddApi(Plus.Api)
                .AddApi(AppStateManager.Api)
                .AddScope(GamesClass.ScopeGames)
                .AddScope(Plus.ScopePlusLogin)
                .AddScope(AppStateManager.ScopeAppState);
            */
            mGoogleApiClient = builder.Build();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate
            {
                count++;
                button.Text = string.Format("{0} clicks!", count);
            };

            Button StartMatch = FindViewById<Button>(Resource.Id.StartMatchBtn);

            StartMatch.Click += delegate
            {
                Toast.MakeText(this, "StartMatch.Click", ToastLength.Long).Show();

                Intent intent = GamesClass.TurnBasedMultiplayer.GetSelectOpponentsIntent(mGoogleApiClient, 1, 1, true);
                StartActivityForResult(intent, RC_SELECT_PLAYERS);
            };

            Button InboxButton = FindViewById<Button>(Resource.Id.InboxBtn);

            InboxButton.Click += delegate
            {
                Toast.MakeText(this, "InboxButton.Click", ToastLength.Long).Show();

                Intent intent = GamesClass.TurnBasedMultiplayer.GetInboxIntent(mGoogleApiClient);
                StartActivityForResult(intent, RC_OPEN_INBOX);
            };

            Button TurnButton = FindViewById<Button>(Resource.Id.TakeTurnBtn);

            TurnButton.Click += delegate
            {
                TakeTurn();
            };
        }

        /// <summary>
        /// Called when the App first starts, after OnCreate is called.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            mGoogleApiClient.Connect();

            Toast.MakeText(this, "Connecting", ToastLength.Long).Show();
        }

        protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult (requestCode, resultCode, data);

            // Check if this was the error code we supplied. That is the only one we care about.
            if (requestCode == REQUEST_CODE_RESOLVE_ERR && resultCode == Result.Ok)
            {
                // TODO: Is google API the only one who sends REQUEST_CODE_RESOLVE_ERR?
                mGoogleApiClient.Connect();
                return;
            }
            else if (requestCode == RC_SELECT_PLAYERS)
            {
                if (resultCode != Result.Ok)
                {
                    Toast.MakeText(this, "Player Did Not Accept Invite", ToastLength.Long).Show();

                    // user canceled
                    return;
                }

                // get the invitee list
                IList<string> invitees = data.GetStringArrayListExtra(Android.Gms.Games.GamesClass.ExtraPlayerIds);

                // get auto-match criteria
                Bundle autoMatchCriteria = null;
                int minAutoMatchPlayers = data.GetIntExtra(Multiplayer.ExtraMinAutomatchPlayers, 0);
                int maxAutoMatchPlayers = data.GetIntExtra(Multiplayer.ExtraMaxAutomatchPlayers, 0);

                if (minAutoMatchPlayers > 0)
                {
                    autoMatchCriteria = RoomConfig.CreateAutoMatchCriteria(minAutoMatchPlayers, maxAutoMatchPlayers, 0);
                }
                else
                {
                    autoMatchCriteria = null;
                }

                TurnBasedMatchConfig tbmc = TurnBasedMatchConfig.InvokeBuilder().AddInvitedPlayers(invitees).SetAutoMatchCriteria(autoMatchCriteria).Build();

                // kick the match off
                GamesClass.TurnBasedMultiplayer.CreateMatch(mGoogleApiClient, tbmc).SetResultCallback(this);
            }
            else if (requestCode == RC_OPEN_INBOX)
            {
                if (resultCode != Result.Ok)
                {
                    Toast.MakeText(this, "Inbox not ok", ToastLength.Long).Show();

                    // user canceled
                    return;
                }
                try
                {
                    mMatch = Java.Lang.Object.GetObject<ITurnBasedMatch>(data.GetParcelableExtra(Multiplayer.ExtraTurnBasedMatch).Handle, JniHandleOwnership.DoNotTransfer);
                    UpdateData();
                }
                catch
                {
                    Toast.MakeText(this, "Failed to cast Match", ToastLength.Long).Show();
                }
            }
        }

        public void OnResult(Java.Lang.Object result)
        {
            try
            {
                ITurnBasedMultiplayerUpdateMatchResult NewResult = Java.Lang.Object.GetObject<ITurnBasedMultiplayerUpdateMatchResult>(result.Handle, JniHandleOwnership.DoNotTransfer);

                if (NewResult != null)
                {
                    mMatch = NewResult.Match;
                    if (mMatch.GetData() != null)
                    {
                        // This is a game that has already started, so I'll just start
                        //updateMatch(match);
                        UpdateData();
                    }
                    return;
                }
            }
            catch
            {
            }

            try
            {
                ITurnBasedMultiplayerInitiateMatchResult NewResult = Java.Lang.Object.GetObject<ITurnBasedMultiplayerInitiateMatchResult>(result.Handle, JniHandleOwnership.DoNotTransfer);

                mMatch = NewResult.Match;
                if (mMatch.GetData() != null)
                {
                    // This is a game that has already started, so I'll just start
                    //updateMatch(match);
                    UpdateData();
                }
                return;
            }
            catch
            {
            }
        }

        private void TakeTurn()
        {
            // Load the game data for this match
            //String mTurnData = new String(mMatch.GetData());

            // Get the next participant in the game-defined way, possibly round-robin.
            //String nextParticipantId = mMatch.pa
            if (mMatch == null)
            {
                Toast.MakeText(this, "No Match", ToastLength.Long).Show();
                return;
            }

            if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatusMyTurn)
            {
                Toast.MakeText(this, "Not your turn!", ToastLength.Long).Show();
                return;
            }

            String nextParticipantId = "";

            foreach (string p in mMatch.ParticipantIds)
            {
                if (p != mMatch.PendingParticipantId)
                {
                    nextParticipantId = p;
                    break;
                }
            }

            // Perform some game action. In this example, we simply retrieve a
            // text string from the view and display a spinner.
            //mTurnData = mDataView.getText().toString();

            // At this point, you might want to show a waiting dialog so that
            // the current player does not try to submit turn actions twice.
            //showSpinner();

            byte[] data = BitConverter.GetBytes(count);

            //startMatch(match);
            GamesClass.TurnBasedMultiplayer.TakeTurn(mGoogleApiClient, mMatch.MatchId, data, nextParticipantId).SetResultCallback(this);
        }

        private void UpdateData()
        {
            if (mMatch != null)
            {
                count = BitConverter.ToInt32(mMatch.GetData(), 0);
                Button button = FindViewById<Button>(Resource.Id.myButton);
                button.Text = string.Format("{0} clicks!", count);
            }
        }

        // BEGIN IGoogleApiClientConnectionCallbacks
        //

        public void OnConnected(Bundle p0)
        {
            Toast.MakeText(this, "OnConnected", ToastLength.Long).Show();

            // Likewise, we are registering the optional MatchUpdateListener, which
            // will replace notifications you would get otherwise. You do *NOT* have
            // to register a MatchUpdateListener.
            GamesClass.TurnBasedMultiplayer.RegisterMatchUpdateListener(mGoogleApiClient, this);
        }

        public void OnConnectionSuspended(int p0)
        {
            Toast.MakeText(this, "OnConnectionSuspended", ToastLength.Long).Show();
        }

        //
        // END IGoogleApiClientConnectionCallbacks

        // BEGIN IGoogleApiClientOnConnectionFailedListener
        //

        public void OnConnectionFailed(ConnectionResult p0)
        {
            Toast.MakeText(this, "OnConnectionFailed", ToastLength.Long).Show();

            ResolveConnectionResult(p0);
        }

        //
        // END IGoogleApiClientOnConnectionFailedListener

        private void ResolveConnectionResult(ConnectionResult result)
        {
            // Does this failure reason have a solution?
            if (result.HasResolution) 
            {
                try 
                {
                    // Try to resolve the problem automatically.
                    result.StartResolutionForResult(this, REQUEST_CODE_RESOLVE_ERR);
                } 
                catch (Android.Content.IntentSender.SendIntentException /*e*/) 
                {
                    // Not really sure why this is here.
                    mGoogleApiClient.Connect();
                }
            }
        }

        // BEGIN IOnTurnBasedMatchUpdateReceivedListener
        //

        public void OnTurnBasedMatchReceived(ITurnBasedMatch match)
        {
            mMatch = match;

            UpdateData();
        }

        public void OnTurnBasedMatchRemoved(string matchId)
        {
        }

        //
        // END IOnTurnBasedMatchUpdateReceivedListener
    }
}


