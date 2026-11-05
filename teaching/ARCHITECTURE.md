# Architecture and the Tracer Bullet

## Follow one shot

Program samples keyboard state once per rendered frame. KeyboardInput converts it to an InputState. The accumulator performs zero or more fixed 1/60-second GameSession steps with that state. Fire is held-state input with a simulation cooldown, so catch-up steps cannot bypass the cooldown.

GameSession creates a bullet with the ship's existing velocity plus muzzle velocity. PhysicsWorld creates its Box2D body and circle shape. Every dynamic body receives a planetary gravity force from the custom library. Box2D integrates those forces and handles contacts. GameSession consumes copied contact pairs, updates score, and removes bodies only after contact iteration.

GameSession queues semantic events, not Sound objects. Program drains them and asks SoundManager to play laser or explosion audio. GameRenderer reads entity positions from Box2D and maps them into sprite rectangles. SpriteManager caches textures by manifest name. The rendered 640 x 360 texture is letterboxed into the window; mouse coordinates are transformed back into that logical canvas.

This is the tracer bullet pattern: one narrow, real end-to-end path establishes the architecture and feedback loop. Later features should extend that path, not replace working systems with speculative infrastructure.

## Physics contract

Distances are metres, elapsed time is seconds, and body masses come from Box2D shape density and area. Rendering uses 18 pixels per metre. The planet is a fixed static body at the origin. Its gravitational parameter mu is 120 in scaled gameplay units; mu means G multiplied by planet mass. The planet does not recoil, and bodies do not gravitationally attract one another.

Gravity acceleration points towards the planet and has magnitude mu / r^2 outside the clamped inner radius. Force is body mass times acceleration. Rocket thrust is a force along the ship's heading with signed throttle. Reverse thrust does not set velocity to zero. Mass and fuel are constant for now.

CircularVelocity returns a tangential initial velocity of magnitude sqrt(mu / r). It is a starting condition, not a constraint that teleports an asteroid around a circle. Box2D owns integration; discrete steps and collisions may perturb orbits. The faint rendered rings are decorative guides, not predicted trajectories. Orbital prediction is a student task.

Screen coordinates are used consistently: +X right, +Y down. Heading 0 points right; +pi/2 points down. Kenney ship artwork points up, so rendering adds 90 degrees. Visual heading is kept separately from the circle collision body's rotation because circle orientation has no collision meaning.

## Ownership and limits

GameSession owns PhysicsWorld. PhysicsWorld owns bodies and its world; entities are references to those handles. Never read an entity position after destroying its body. Box2D's event buffers are transient; copy what is needed before any destruction.

Program owns the window/render texture. SpriteManager owns loaded textures. SoundManager owns sound handles and the audio device. They are disposed before the window closes. Missing assets fail visibly; unavailable audio allows silent gameplay.

The loop caps stalled-frame catch-up at 0.25 seconds and does not interpolate render positions. There is no camera tracking, screen wrapping, fuel, orbital prediction, enemy AI, save game, or general scene system. These are boundaries of the prototype, not disguised implementations.

## Add another tracer bullet

For a fuel feature, first thread one observable value through input -> thrust -> fuel model -> HUD -> restart and a focused test. Once that works, add capacity, refill, and balancing. For a new sound or sprite, first add one manifest entry, one build Content entry, and one working call site.
