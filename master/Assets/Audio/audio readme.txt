HOW THE AUDIO SHOULD PLAY!

WIND AMBIENCE:
	all of the wind sounds should always be looping
	amb_wind_calm should always be at 100% volume
	Keep amb_wind_blowy and amb_wind_veryblowy at 0% vol until you want the wind to be blowing, then just increase them as needed
	that's all!

WAVES AMBIENCE:
	amb_waves_loop should always be looping. The volume could be lowered by ~25% when we're on an island
	amb_waves_shore should be a looping emitter placed every few meters along the shores of islands. Short attenuation distance, like 10-20m

MUSIC:
	Keep both day and night music looping constantly
	When transitioning betweeen island and boat, fade out (7 seconds) the current track, then fade in (5 seconds) the other one

GULLS:
	On islands in day time, every 5 seconds, 50% chance to play a random gull sound, attenuation distance should be long, like 500m

FOOTSTEPS:
	Select one at random when the player moves on land, emitting from the player character, that's all!