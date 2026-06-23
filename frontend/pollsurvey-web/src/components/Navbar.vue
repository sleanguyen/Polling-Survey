<template>
  <nav class="navbar">
    <div class="nav-inner container">
      <RouterLink to="/" class="nav-logo">
        <span class="logo-mark">P</span>
        <span class="logo-text">PollSurvey</span>
      </RouterLink>
      <RouterLink to="/create" class="nav-cta">
        + New Poll
      </RouterLink>
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  position: sticky;
  top: 0;
  z-index: 50;
  background: var(--color-accent);
  border-bottom: 1px solid rgba(0,0,0,0.18);
  box-shadow: 0 2px 12px rgba(107,14,30,0.2);
}
.nav-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 58px;
}
.nav-logo {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  text-decoration: none;
}
.logo-mark {
  width: 30px;
  height: 30px;
  border-radius: 8px;
  background: rgba(245,222,141,0.18);
  border: 1px solid rgba(245,222,141,0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-display);
  font-size: 1rem;
  font-weight: 700;
  color: #f5de8d;
}
.logo-text {
  font-family: var(--font-display);
  font-size: 1.15rem;
  font-weight: 600;
  color: #f5de8d;
  letter-spacing: 0.01em;
}
.nav-cta {
  font-family: var(--font-body);
  font-size: 0.85rem;
  font-weight: 700;
  padding: 0.45rem 1.1rem;
  border-radius: var(--radius-sm);
  background: #f5de8d;
  color: var(--color-accent);
  border: none;
  cursor: pointer;
  letter-spacing: 0.01em;
  text-decoration: none;
  transition: background var(--transition), transform var(--transition);
}
.nav-cta:hover {
  background: #fff8e8;
  transform: translateY(-1px);
  text-decoration: none;
}
</style>
