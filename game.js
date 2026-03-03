const canvas = document.getElementById('game');
const ctx = canvas.getContext('2d');

const hud = {
  lives: document.getElementById('lives'),
  coins: document.getElementById('coins'),
  power: document.getElementById('power'),
  timer: document.getElementById('timer'),
  world: document.getElementById('world'),
};

const keys = new Set();
window.addEventListener('keydown', (e) => keys.add(e.key.toLowerCase()));
window.addEventListener('keyup', (e) => keys.delete(e.key.toLowerCase()));

const WORLD_WIDTH = 7000;
const GRAVITY = 2200;

const powerups = {
  normal: { name: 'Normal', projectile: null, defense: 1, slideBoost: 0 },
  blaze: { name: 'Blaze Fruit', projectile: 'fire', defense: 1, slideBoost: 0 },
  frost: { name: 'Frost Cap', projectile: 'frost', defense: 1, slideBoost: 0 },
  shell: { name: 'Shell Armor', projectile: null, defense: 0.6, slideBoost: 140 },
};

const state = {
  lives: 3,
  coins: 0,
  timer: 300,
  worldName: 'Aurora Fields',
  timeAcc: 0,
  power: powerups.normal,
  doubleJumpUnlocked: false,
  cameraX: 0,
  levelComplete: false,
};

const player = {
  x: 90, y: 100, w: 44, h: 58,
  vx: 0, vy: 0,
  onGround: false,
  facing: 1,
  coyote: 0,
  jumpBuffer: 0,
  jumpHold: 0,
  wallSliding: false,
  wallDir: 0,
  didDoubleJump: false,
  groundPound: false,
  invuln: 0,
};

const platforms = [
  {x: 0, y: 650, w: 1600, h: 100},
  {x: 1700, y: 610, w: 500, h: 140},
  {x: 2300, y: 570, w: 420, h: 180},
  {x: 2900, y: 620, w: 780, h: 130},
  {x: 3900, y: 670, w: 600, h: 80},
  {x: 4750, y: 620, w: 700, h: 130},
  {x: 5600, y: 680, w: 1400, h: 70},
  {x: 2500, y: 450, w: 180, h: 30},
  {x: 3150, y: 500, w: 180, h: 30},
  {x: 5200, y: 520, w: 180, h: 30},
];

const pipes = [
  {x: 1200, y: 560, w: 90, h: 90, target: {x: 2440, y: 380}},
  {x: 5400, y: 590, w: 90, h: 90, target: {x: 1300, y: 520}},
];

const coins = Array.from({length: 28}, (_, i) => ({
  x: 250 + i * 220,
  y: i % 4 === 0 ? 450 : 560,
  taken: false,
  secret: i % 7 === 0,
}));

const projectiles = [];
const enemies = [
  {type: 'walker', x: 700, y: 610, w: 42, h: 40, vx: 80, hp: 1, frozen: 0},
  {type: 'walker', x: 2100, y: 570, w: 42, h: 40, vx: -90, hp: 1, frozen: 0},
  {type: 'flyer', x: 2670, y: 330, w: 40, h: 34, phase: 0, hp: 1, frozen: 0},
  {type: 'hopper', x: 3380, y: 580, w: 44, h: 40, vy: 0, jumpCd: 0.8, hp: 2, frozen: 0},
  {type: 'walker', x: 4500, y: 630, w: 42, h: 40, vx: 75, hp: 2, frozen: 0, armor: true},
];

const boss = {
  active: true,
  x: 6460, y: 560, w: 120, h: 120,
  vx: -80,
  hp: 12,
  phase: 0,
  patternCd: 1.6,
  weak: false,
  weakTimer: 0,
};

function intersects(a, b) {
  return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
}

function platformAt(rect) {
  return platforms.find(p => intersects(rect, p));
}

function setPower(name) {
  state.power = powerups[name];
  hud.power.textContent = state.power.name;
}

function spawnProjectile(type) {
  if (!type) return;
  projectiles.push({
    type,
    x: player.x + player.w / 2,
    y: player.y + player.h / 2,
    w: 16,
    h: 16,
    vx: 520 * player.facing,
    life: 1.8,
  });
}

function hitPlayer(raw = 1) {
  if (player.invuln > 0) return;
  const damage = Math.max(1, Math.round(raw * state.power.defense));
  state.lives -= damage;
  player.invuln = 1.2;
  if (state.lives <= 0) {
    state.lives = 3;
    state.coins = 0;
    state.timer = 300;
    player.x = 90; player.y = 100; player.vx = 0; player.vy = 0;
  }
}

function update(dt) {
  if (state.levelComplete) return;

  state.timeAcc += dt;
  if (state.timeAcc > 1) {
    state.timeAcc = 0;
    state.timer--;
    if (state.timer <= 0) {
      state.timer = 300;
      hitPlayer(1);
    }
  }

  if (keys.has('1')) setPower('blaze');
  if (keys.has('2')) setPower('frost');
  if (keys.has('3')) setPower('shell');
  if (keys.has('0')) setPower('normal');
  if (keys.has('u')) state.doubleJumpUnlocked = true;

  player.invuln = Math.max(0, player.invuln - dt);
  player.coyote = player.onGround ? 0.1 : Math.max(0, player.coyote - dt);
  player.jumpBuffer = Math.max(0, player.jumpBuffer - dt);

  const left = keys.has('a') || keys.has('arrowleft');
  const right = keys.has('d') || keys.has('arrowright');
  const jumpPressed = keys.has(' ');
  const attack = keys.has('j');
  const spin = keys.has('k');
  const pound = (keys.has('s') || keys.has('arrowdown')) && !player.onGround;

  const target = (right ? 1 : 0) - (left ? 1 : 0);
  if (target !== 0) player.facing = Math.sign(target);

  const maxRun = 370 + state.power.slideBoost;
  const accel = player.onGround ? 2600 : 1450;
  const decel = player.onGround ? 2200 : 1100;

  if (target !== 0) {
    player.vx += target * accel * dt;
  } else {
    player.vx -= Math.sign(player.vx) * Math.min(Math.abs(player.vx), decel * dt);
  }
  player.vx = Math.max(-maxRun, Math.min(maxRun, player.vx));

  if (jumpPressed) player.jumpBuffer = 0.12;
  const jumpHold = keys.has(' ');

  if (player.jumpBuffer > 0 && (player.coyote > 0 || player.onGround)) {
    player.vy = -760;
    player.jumpHold = 0.2;
    player.onGround = false;
    player.jumpBuffer = 0;
  } else if (player.jumpBuffer > 0 && player.wallSliding) {
    player.vy = -700;
    player.vx = 480 * -player.wallDir;
    player.jumpHold = 0.15;
    player.wallSliding = false;
    player.jumpBuffer = 0;
  } else if (player.jumpBuffer > 0 && state.doubleJumpUnlocked && !player.didDoubleJump) {
    player.vy = -700;
    player.didDoubleJump = true;
    player.jumpBuffer = 0;
  }

  if (player.groundPound && player.onGround) player.groundPound = false;
  if (pound && !player.groundPound) {
    player.groundPound = true;
    player.vx = 0;
    player.vy = 1100;
  }

  const gravityScale = (player.vy < 0 && jumpHold && player.jumpHold > 0) ? 0.48 : (player.groundPound ? 2.2 : 1.0);
  player.jumpHold = Math.max(0, player.jumpHold - dt);
  player.vy += GRAVITY * gravityScale * dt;

  player.x += player.vx * dt;
  player.y += player.vy * dt;

  player.onGround = false;
  for (const p of platforms) {
    if (intersects(player, p)) {
      const prevBottom = player.y - player.vy * dt + player.h;
      if (prevBottom <= p.y + 8 && player.vy >= 0) {
        player.y = p.y - player.h;
        player.vy = 0;
        player.onGround = true;
        player.didDoubleJump = false;
      } else if (player.x + player.w / 2 < p.x + p.w / 2) {
        player.x = p.x - player.w;
        player.vx = Math.min(0, player.vx);
      } else {
        player.x = p.x + p.w;
        player.vx = Math.max(0, player.vx);
      }
    }
  }

  player.wallSliding = false;
  player.wallDir = 0;
  if (!player.onGround && player.vy > 0) {
    for (const p of platforms) {
      if (player.y + player.h > p.y + 12 && player.y < p.y + p.h) {
        if (Math.abs((player.x + player.w) - p.x) < 6) { player.wallSliding = true; player.wallDir = 1; }
        if (Math.abs(player.x - (p.x + p.w)) < 6) { player.wallSliding = true; player.wallDir = -1; }
      }
    }
  }
  if (player.wallSliding) player.vy = Math.min(player.vy, 120);

  if (player.y > 900) {
    hitPlayer(1);
    player.x = Math.max(0, player.x - 160);
    player.y = 100;
    player.vx = player.vy = 0;
  }

  if (attack) spawnProjectile(state.power.projectile);

  for (const pr of projectiles) {
    pr.x += pr.vx * dt;
    pr.life -= dt;
  }
  for (const e of enemies) {
    if (e.hp <= 0) continue;
    e.frozen = Math.max(0, e.frozen - dt);
    if (e.frozen > 0) continue;
    if (e.type === 'walker') {
      e.x += e.vx * dt;
      if (!platformAt({x: e.x, y: e.y + e.h + 4, w: e.w, h: 2})) e.vx *= -1;
    } else if (e.type === 'flyer') {
      e.phase += dt * 2.5;
      e.x += 95 * dt;
      e.y += Math.sin(e.phase) * 70 * dt;
    } else if (e.type === 'hopper') {
      e.jumpCd -= dt;
      if (e.jumpCd <= 0) {
        e.jumpCd = 1.1;
        e.vy = -620;
        e.x += (player.x > e.x ? 1 : -1) * 70;
      }
      e.vy += GRAVITY * dt;
      e.y += e.vy * dt;
      const p = platformAt({x: e.x, y: e.y, w: e.w, h: e.h});
      if (p && e.vy > 0) {
        e.y = p.y - e.h;
        e.vy = 0;
      }
    }
  }

  for (const pr of projectiles) {
    if (pr.life <= 0) continue;
    for (const e of enemies) {
      if (e.hp > 0 && intersects(pr, e)) {
        pr.life = 0;
        if (e.armor && pr.type === 'frost') e.frozen = 2.8;
        else if (!e.armor || pr.type !== 'fire') e.hp -= 1;
      }
    }
    if (boss.active && intersects(pr, boss) && boss.weak) {
      boss.hp -= 1;
      pr.life = 0;
    }
  }

  if (spin) {
    for (const e of enemies) {
      const dx = (e.x + e.w / 2) - (player.x + player.w / 2);
      const dy = (e.y + e.h / 2) - (player.y + player.h / 2);
      if (e.hp > 0 && dx * dx + dy * dy < 130 * 130) e.hp -= 1;
    }
  }

  for (const e of enemies) {
    if (e.hp <= 0) continue;
    if (intersects(player, e)) {
      const stomp = player.vy > 0 && player.y + player.h - 10 < e.y;
      if (stomp && !e.armor) {
        e.hp -= 1;
        player.vy = -520;
      } else {
        hitPlayer(1);
      }
    }
  }

  if (boss.active) {
    boss.patternCd -= dt;
    if (boss.patternCd <= 0) {
      boss.phase = (boss.phase + 1) % 3;
      boss.patternCd = 1.2 - Math.min(0.5, (12 - boss.hp) * 0.04);
      boss.weak = true;
      boss.weakTimer = 0.8;
      boss.vx = boss.phase === 1 ? 240 : -100;
    }
    if (boss.weak) {
      boss.weakTimer -= dt;
      if (boss.weakTimer <= 0) boss.weak = false;
    }
    boss.x += boss.vx * dt;
    if (boss.x < 6200 || boss.x > 6900) boss.vx *= -1;

    if (intersects(player, boss)) {
      const stomp = player.vy > 0 && player.y + player.h - 14 < boss.y && boss.weak;
      if (stomp) {
        boss.hp -= 1;
        player.vy = -620;
      } else hitPlayer(1);
    }

    if (boss.hp <= 0) {
      boss.active = false;
      state.levelComplete = true;
      state.worldName = 'Victory at Ember Bastion';
    }
  }

  for (const c of coins) {
    if (!c.taken && intersects(player, {x: c.x, y: c.y, w: 18, h: 18})) {
      c.taken = true;
      state.coins += c.secret ? 5 : 1;
    }
  }

  for (const pipe of pipes) {
    if (intersects(player, pipe) && (keys.has('w') || keys.has('arrowup'))) {
      player.x = pipe.target.x;
      player.y = pipe.target.y;
      player.vx = player.vy = 0;
    }
  }

  player.x = Math.max(0, Math.min(WORLD_WIDTH - player.w, player.x));
  state.cameraX += ((player.x - 260) - state.cameraX) * 0.12;
  state.cameraX = Math.max(0, Math.min(WORLD_WIDTH - canvas.width, state.cameraX));

  hud.lives.textContent = state.lives;
  hud.coins.textContent = state.coins;
  hud.timer.textContent = state.timer;
  hud.world.textContent = state.worldName;

  for (let i = projectiles.length - 1; i >= 0; i--) if (projectiles[i].life <= 0) projectiles.splice(i, 1);
}

function draw() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  const cam = state.cameraX;
  const parallax = [0.2, 0.5, 0.8];
  const colors = ['#c8eeff', '#98dcff', '#6fc3ff'];
  for (let i = 0; i < parallax.length; i++) {
    const px = -(cam * parallax[i]) % canvas.width;
    ctx.fillStyle = colors[i];
    ctx.fillRect(px, 280 + i * 80, canvas.width, 240);
    ctx.fillRect(px - canvas.width, 280 + i * 80, canvas.width, 240);
  }

  const drawRect = (o, color) => {
    ctx.fillStyle = color;
    ctx.fillRect(o.x - cam, o.y, o.w, o.h);
  };

  for (const p of platforms) drawRect(p, '#4c9141');
  for (const pipe of pipes) drawRect(pipe, '#2a6a2f');

  for (const c of coins) {
    if (c.taken) continue;
    ctx.beginPath();
    ctx.fillStyle = c.secret ? '#ff7be8' : '#ffd53d';
    ctx.arc(c.x - cam + 9, c.y + 9, 9, 0, Math.PI * 2);
    ctx.fill();
  }

  for (const e of enemies) {
    if (e.hp <= 0) continue;
    drawRect(e, e.frozen > 0 ? '#89d2ff' : (e.armor ? '#4a4a4a' : '#bb3d1f'));
  }

  if (boss.active) {
    drawRect(boss, boss.weak ? '#9b39ff' : '#5c1f99');
    ctx.fillStyle = '#fff';
    ctx.fillText(`Boss HP: ${boss.hp}`, 980, 30);
  }

  for (const pr of projectiles) drawRect(pr, pr.type === 'fire' ? '#ff6a2a' : '#6fe8ff');

  drawRect(player, player.invuln > 0 ? '#ffd9a6' : '#f2a95f');

  if (state.levelComplete) {
    ctx.fillStyle = 'rgba(0,0,0,.55)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#fff';
    ctx.font = '54px sans-serif';
    ctx.fillText('Stage Clear!', 470, 320);
    ctx.font = '28px sans-serif';
    ctx.fillText('You defeated the Ember Bastion boss.', 430, 370);
  }
}

let last = performance.now();
function frame(now) {
  const dt = Math.min(0.033, (now - last) / 1000);
  last = now;
  update(dt);
  draw();
  requestAnimationFrame(frame);
}

setPower('normal');
requestAnimationFrame(frame);
