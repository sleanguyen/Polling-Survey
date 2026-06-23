<template>
  <nav class="navbar">
    <div class="nav-inner container">
      <RouterLink to="/" class="nav-logo">
        <span class="logo-dot"></span>
        PollSurvey
      </RouterLink>
      <RouterLink to="/create" class="btn btn-primary btn-sm">
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
  border-bottom: 1px solid rgba(0,0,0,0.15);
}
.nav-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 56px;
}
.nav-logo {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  font-size: 1.05rem;
  font-weight: 700;
  color: #f5de8d;
  text-decoration: none;
}
.logo-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #f5de8d;
  box-shadow: 0 0 8px rgba(245,222,141,0.5);
}
.btn-sm {
  padding: 0.45rem 1rem;
  font-size: 0.85rem;
  background: #f5de8d !important;
  color: var(--color-accent) !important;
}
.btn-sm:hover { opacity: 0.88; }
</style>
