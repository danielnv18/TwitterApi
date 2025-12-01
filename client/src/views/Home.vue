<script setup lang="ts">
</script>

<template>
  <div>
    <!-- Header -->
    <div class="sticky top-0 bg-black/80 backdrop-blur-md border-b border-gray-800 p-4 z-10">
      <h1 class="text-xl font-bold">Home</h1>
    </div>

    <!-- Tweet Input -->
    <div class="border-b border-gray-800 p-4">
      <div class="flex space-x-4">
        <div class="w-10 h-10 bg-gray-700 rounded-full flex-shrink-0"></div>
        <div class="flex-1">
          <input 
            type="text" 
            placeholder="What is happening?!" 
            class="bg-transparent w-full text-xl outline-none placeholder-gray-500 mb-4"
          >
          <div class="flex justify-end">
            <button class="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded-full disabled:opacity-50">
              Post
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Feed -->
    <div class="divide-y divide-gray-800">
      <div v-for="i in 5" :key="i" class="p-4 hover:bg-gray-900/50 transition cursor-pointer">
        <div class="flex space-x-4">
          <div class="w-10 h-10 bg-gray-700 rounded-full flex-shrink-0"></div>
          <div>
            <div class="flex items-center space-x-2">
              <span class="font-bold">User {{ i }}</span>
              <span class="text-gray-500">@user{{ i }}</span>
              <span class="text-gray-500">· 1h</span>
            </div>
            <p class="mt-1">This is a sample tweet content to demonstrate the layout. #twitterclone</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
