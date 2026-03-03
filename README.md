# Starbound Sprint (HTML5) – 2.5D Side-Scroller Prototype

This project is now **HTML-based** and runs directly in the browser using Canvas + JavaScript (no Unity/Unreal required).

## Download and Play

1. Clone or download the repository.
2. Open `index.html` in a modern browser.

For best compatibility, serve it with a tiny local web server:

```bash
python3 -m http.server 8080
```

Then open: `http://localhost:8080`

## Implemented Gameplay

- Momentum-driven running and jumping.
- Variable jump height (short vs long press).
- Wall sliding and wall jumping.
- Ground pound.
- Optional double jump unlock (`U`).
- Spin attack (`K`) and projectile attacks (`J`).
- Power-ups:
  - `1` Blaze Fruit (fire projectiles)
  - `2` Frost Cap (freeze-style projectile behavior)
  - `3` Shell Armor (defense + slide speed boost)
  - `0` Normal form
- Enemies: walker, flyer, hopper.
- Boss arena encounter with weak-window phases.
- Coins (including secret-value coins), warp pipes, timer, lives, HUD.
- Layered parallax background for a 2.5D look.

## Controls

- Move: `A/D` or `←/→`
- Jump: `Space`
- Attack Projectile: `J`
- Spin Attack: `K`
- Ground Pound: `S` or `↓` while airborne
- Pipe Warp: hold `W` or `↑` while touching a pipe

## Files

- `index.html` – page shell, HUD, canvas.
- `styles.css` – visual styling and layout.
- `game.js` – game loop, physics, AI, combat, rendering.
