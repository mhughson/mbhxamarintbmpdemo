package coponenttest;


public class MainActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.common.api.GoogleApiClient.ConnectionCallbacks,
		com.google.android.gms.common.api.GoogleApiClient.OnConnectionFailedListener,
		com.google.android.gms.common.GooglePlayServicesClient.OnConnectionFailedListener,
		com.google.android.gms.common.api.ResultCallback,
		com.google.android.gms.games.multiplayer.turnbased.OnTurnBasedMatchUpdateReceivedListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onStart:()V:GetOnStartHandler\n" +
			"n_onActivityResult:(IILandroid/content/Intent;)V:GetOnActivityResult_IILandroid_content_Intent_Handler\n" +
			"n_onConnected:(Landroid/os/Bundle;)V:GetOnConnected_Landroid_os_Bundle_Handler:Android.Gms.Common.Apis.IGoogleApiClientConnectionCallbacksInvoker, GooglePlayServicesLib\n" +
			"n_onConnectionSuspended:(I)V:GetOnConnectionSuspended_IHandler:Android.Gms.Common.Apis.IGoogleApiClientConnectionCallbacksInvoker, GooglePlayServicesLib\n" +
			"n_onConnectionFailed:(Lcom/google/android/gms/common/ConnectionResult;)V:GetOnConnectionFailed_Lcom_google_android_gms_common_ConnectionResult_Handler:Android.Gms.Common.IGooglePlayServicesClientOnConnectionFailedListenerInvoker, GooglePlayServicesLib\n" +
			"n_onResult:(Lcom/google/android/gms/common/api/Result;)V:GetOnResult_Lcom_google_android_gms_common_api_Result_Handler:Android.Gms.Common.Apis.IResultCallbackInvoker, GooglePlayServicesLib\n" +
			"n_onTurnBasedMatchReceived:(Lcom/google/android/gms/games/multiplayer/turnbased/TurnBasedMatch;)V:GetOnTurnBasedMatchReceived_Lcom_google_android_gms_games_multiplayer_turnbased_TurnBasedMatch_Handler:Android.Gms.Games.MultiPlayer.TurnBased.IOnTurnBasedMatchUpdateReceivedListenerInvoker, GooglePlayServicesLib\n" +
			"n_onTurnBasedMatchRemoved:(Ljava/lang/String;)V:GetOnTurnBasedMatchRemoved_Ljava_lang_String_Handler:Android.Gms.Games.MultiPlayer.TurnBased.IOnTurnBasedMatchUpdateReceivedListenerInvoker, GooglePlayServicesLib\n" +
			"";
		mono.android.Runtime.register ("CoponentTest.MainActivity, CoponentTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MainActivity.class, __md_methods);
	}


	public MainActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == MainActivity.class)
			mono.android.TypeManager.Activate ("CoponentTest.MainActivity, CoponentTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onStart ()
	{
		n_onStart ();
	}

	private native void n_onStart ();


	public void onActivityResult (int p0, int p1, android.content.Intent p2)
	{
		n_onActivityResult (p0, p1, p2);
	}

	private native void n_onActivityResult (int p0, int p1, android.content.Intent p2);


	public void onConnected (android.os.Bundle p0)
	{
		n_onConnected (p0);
	}

	private native void n_onConnected (android.os.Bundle p0);


	public void onConnectionSuspended (int p0)
	{
		n_onConnectionSuspended (p0);
	}

	private native void n_onConnectionSuspended (int p0);


	public void onConnectionFailed (com.google.android.gms.common.ConnectionResult p0)
	{
		n_onConnectionFailed (p0);
	}

	private native void n_onConnectionFailed (com.google.android.gms.common.ConnectionResult p0);


	public void onResult (com.google.android.gms.common.api.Result p0)
	{
		n_onResult (p0);
	}

	private native void n_onResult (com.google.android.gms.common.api.Result p0);


	public void onTurnBasedMatchReceived (com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch p0)
	{
		n_onTurnBasedMatchReceived (p0);
	}

	private native void n_onTurnBasedMatchReceived (com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch p0);


	public void onTurnBasedMatchRemoved (java.lang.String p0)
	{
		n_onTurnBasedMatchRemoved (p0);
	}

	private native void n_onTurnBasedMatchRemoved (java.lang.String p0);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
