using Sandbox;

using Facepunch.Minigolf.Entities;
using Facepunch.Minigolf.UI;

namespace Facepunch.Minigolf;

public partial class Game : Sandbox.Game
{
	public static new Game Current => Sandbox.Game.Current as Game;

	public Game()
	{
		if ( IsServer )
		{
			AddToPrecache();
			Course = new();
		}

		if ( IsClient )
		{
			Local.Hud = new GolfRootPanel();
		}
	}

	public override void ClientJoined( Client cl )
	{
		Log.Info( $"\"{cl.Name}\" has joined the game" );

		if ( State == GameState.Playing )
		{
			cl.SetValue( "late", true );
			ChatBox.AddInformation( To.Everyone, $"{ cl.Name } has joined late, they will not be eligible for scoring.", $"avatar:{ cl.PlayerId }" );

			// Just give them shitty scores on each hole for now
			for ( int i = 0; i < Course._currentHole; i++ )
				cl.SetInt( $"par_{ i }", Course.Holes[i].Par + 1 );
		}
		else
		{
			ChatBox.AddInformation( To.Everyone, $"{ cl.Name } has joined", $"avatar:{ cl.PlayerId }" );
		}
	}

	public override bool CanHearPlayerVoice(Client source, Client dest) => true;

	public override void PostLevelLoaded()
	{
		StartTime = Time.Now + 60.0f;
		Course.LoadFromMap();
	}

	public override void BuildInput( InputBuilder input )
	{
		Host.AssertClient();

		Event.Run( "buildinput", input );

		// todo: pass to spectate

		if ( input.Pressed( InputButton.View ) && Local.Pawn.IsValid() && !(Local.Pawn as Ball).InPlay && !(Local.Pawn as Ball).Cupped && FreeCamTimeLeft > 0.0f )
		{
			if ( FreeCamera == null )
				FreeCamera = new FreeCamera();
			else
				FreeCamera = null;
		}

		// the camera is the primary method here
		var camera = FindActiveCamera();
		camera?.BuildInput( input );

		Local.Pawn?.BuildInput( input );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.Pawn is Ball ball && !ball.Cupped )
		{
			if ( Input.Pressed( InputButton.Reload ) )
				ResetBall( cl );
		}
	}
}