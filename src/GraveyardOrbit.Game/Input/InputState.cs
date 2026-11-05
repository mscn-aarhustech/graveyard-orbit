namespace GraveyardOrbit.Game.Input;

public readonly record struct InputState(float Throttle, float Turn, bool Fire, bool Boost, bool Precision);
