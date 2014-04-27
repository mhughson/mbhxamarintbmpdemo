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

        // Keeps track of all the information about this game, and the IMatch it is associated
        // with. This is game specific data. In our case we just store the number of times
        // a button has been clicked.
        class MatchInfo
        {
            // How many times has the button been clicked?
            int mClickCount;

            // Keep a reference to the IMatch so that we can check stuff on it.
            ITurnBasedMatch mMatch;

            MainActivity mOwningActivity;

            public MatchInfo(MainActivity owner)
            {
                mOwningActivity = owner;
            }

            public void UpdateMatchInfo(ITurnBasedMatch match)
            {
                mMatch = match;

                if (mMatch != null && mMatch.GetData() != null)
                {
                    pClickCount = BitConverter.ToInt32(mMatch.GetData(), 0);
                }
                else
                {
                    pClickCount = 0;
                }
            }

            public void IncrementClickCount(int amount = 1)
            {
                if (mMatch == null)
                {
                    Toast.MakeText(mOwningActivity, "Start Match First", ToastLength.Long).Show();
                    return;
                }

                if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatusMyTurn)
                {
                    Toast.MakeText(mOwningActivity, "Not Your Turn", ToastLength.Long).Show();
                    return;
                }

                pClickCount += amount;
            }

            public void EndTurn()
            {
                if (mMatch == null)
                {
                    Toast.MakeText(mOwningActivity, "Start Match First", ToastLength.Long).Show();
                    return;
                }

                if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatusMyTurn)
                {
                    Toast.MakeText(mOwningActivity, "Not Your Turn", ToastLength.Long).Show();
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

                byte[] data = BitConverter.GetBytes(pClickCount);

                //startMatch(match);
                GamesClass.TurnBasedMultiplayer.TakeTurn(mOwningActivity.mGoogleApiClient, mMatch.MatchId, data, nextParticipantId).SetResultCallback(mOwningActivity);
            }

            public int pClickCount
            {
                get
                {
                    return mClickCount;
                }
                set
                {
                    mClickCount = value;
                    TextView GameState = mOwningActivity.FindViewById<TextView>(Resource.Id.GameState);
                    GameState.Text = mClickCount.ToString();
                }
            }

            public ITurnBasedMatch pMatch
            {
                get
                {
                    return mMatch;
                }
            }
        }

        MatchInfo mMatch;

        // Our gateway to all things Google.
        public IGoogleApiClient mGoogleApiClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //mGoogleApiClient = new GoogleApiClientBuilder(this).AddApi(Plus.Api).AddScope(new Scope(Scopes.Games)).Build();

            // create an instance of Google API client and specify the Play services 
            // and scopes to use. In this example, we specify that the app wants 
            // access to the Games, Plus, and Cloud Save services and scopes.
            GoogleApiClientBuilder builder = new GoogleApiClientBuilder(this, this, this);
            builder.AddApi(GamesClass.Api).AddScope(GamesClass.ScopeGames);
            mGoogleApiClient = builder.Build();

            mMatch = new MatchInfo(this);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);
            button.Click += delegate
            {
                mMatch.IncrementClickCount();
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
                mMatch.EndTurn();
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
                    ITurnBasedMatch match = Java.Lang.Object.GetObject<ITurnBasedMatch>(data.GetParcelableExtra(Multiplayer.ExtraTurnBasedMatch).Handle, JniHandleOwnership.DoNotTransfer);
                    mMatch.UpdateMatchInfo(match);
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
                    mMatch.UpdateMatchInfo(NewResult.Match);
                    return;
                }
            }
            catch
            {
            }

            try
            {
                ITurnBasedMultiplayerInitiateMatchResult NewResult = Java.Lang.Object.GetObject<ITurnBasedMultiplayerInitiateMatchResult>(result.Handle, JniHandleOwnership.DoNotTransfer);

                if (NewResult != null)
                {
                    mMatch.UpdateMatchInfo(NewResult.Match);
                    return;
                }
            }
            catch
            {
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
                    mGoogleApiClient.Connect();
                }
            }
        }

        // BEGIN IOnTurnBasedMatchUpdateReceivedListener
        //

        public void OnTurnBasedMatchReceived(ITurnBasedMatch match)
        {
            mMatch.UpdateMatchInfo(match);
        }

        public void OnTurnBasedMatchRemoved(string matchId)
        {
        }

        //
        // END IOnTurnBasedMatchUpdateReceivedListener
    }
}


