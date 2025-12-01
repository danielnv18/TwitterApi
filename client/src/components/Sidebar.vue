<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <nav class="flex flex-col space-y-4">
    <div class="text-3xl font-bold px-4 py-2">
      X
    </div>
    
    <RouterLink to="/" class="flex items-center space-x-4 px-4 py-3 hover:bg-gray-900 rounded-full transition text-xl">
      <span>🏠</span>
      <span>Home</span>
    </RouterLink>

    <RouterLink to="/profile/me" class="flex items-center space-x-4 px-4 py-3 hover:bg-gray-900 rounded-full transition text-xl">
      <span>👤</span>
      <span>Profile</span>
    </RouterLink>

    <button class="bg-blue-500 hover:bg-blue-600 text-white font-bold py-3 px-8 rounded-full mt-4 w-full">
      Post
    </button>
  </nav>
</template>
