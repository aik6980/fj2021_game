HOW THE AUDIO SHOULD PLAY!

WIND AMBIENCE:
	all of the wind sounds should always be looping
	amb_wind_calm should always be at 100% volume
	Keep amb_wind_blowy and amb_wind_veryblowy at 0% vol until you want the wind to be blowing, then just increase them as needed
	that's all!

WAVES AMBIENCE:
	amb_waves_loop should always be looping. The volume could be lowered by ~25% when we're on an island
	amb_waves_shore should be a looping emitter placed every few meters along the shores of islands. Short attenuation distance, like 10-20m

GULLS:
	On islands in day time, every 5 seconds, 50% chance to play a random gull sound, attenuation distance should be long, like 500m

CRICKETS:
	On islands at night loop it. attenuation distance should just cover the island.

FOOTSTEPS:
	Select one at random when the player moves on land, emitting from the player character, that's all!

STARGAZING:
	When the player clicks on a star, play sfx_stargazing_start
	while the mouse button is held, loop sfx_stargazing_loop
	when the mouse button is released on a star, play sfx_stargazing_end
	when a constellation is finished, play sfx_stargazing_constellation

MUSIC:
	Keep stargazing music looping constantly, just on a playlist between 1 and 2
	When transitioning to stargazing, fade out sailing music and fade in stargazing. After 10 seconds, stop sailing music
	When transitioning to sailing, fade out stargazing. If sailing is still playing fade it in, otherwise start it from the start.
	Start sailing music when player first presses 'move forward' while on boat. 
	Sailing music follows a playlist of (1a or 1b), then (2a or 2b), then 3. Might be a bit out of scope to have actual adaptive music at this stage, but maybe for polish would be nice :)