<script setup lang="ts">
import Sidebar from './Sidebar.vue'
</script>

<template>
  <div class="min-h-screen bg-black text-white">
    <div class="container mx-auto grid grid-cols-1 md:grid-cols-4 gap-4">
      <!-- Sidebar -->
      <div class="hidden md:block col-span-1 border-r border-gray-800 min-h-screen p-4">
        <Sidebar />
      </div>

      <!-- Main Content -->
      <div class="col-span-1 md:col-span-2 border-r border-gray-800 min-h-screen">
        <router-view />
      </div>

      <!-- Right Bar (Widgets) -->
      <div class="hidden md:block col-span-1 p-4">
        <div class="bg-gray-900 rounded-xl p-4">
          <h2 class="font-bold text-xl mb-4">Who to follow</h2>
          <!-- Placeholder for suggestions -->
          <div class="space-y-4">
            <div class="flex items-center space-x-2">
              <div class="w-10 h-10 bg-gray-700 rounded-full"></div>
              <div>
                <p class="font-bold">User 1</p>
                <p class="text-gray-500 text-sm">@user1</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
