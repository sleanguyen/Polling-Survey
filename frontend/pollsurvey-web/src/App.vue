<template>
  <div id="app">
    <Navbar />
    <RouterView />
  </div>
</template>

<script setup>
import Navbar from '@/components/Navbar.vue'
</script>

<style scoped>
#app { min-height: 100vh; display: flex; flex-direction: column; }
</style>
