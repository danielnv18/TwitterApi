<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import api from '../services/api'

const route = useRoute()
const username = ref(route.params.username as string || 'me')
const user = ref<any>(null)
const loading = ref(true)
const error = ref('')

const fetchProfile = async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await api.get(`/users/${username.value}`)
    user.value = response.data
  } catch (e: any) {
    console.error(e)
    error.value = 'Failed to load profile'
  } finally {
    loading.value = false
  }
}

onMounted(fetchProfile)

const editing = ref(false)
const editForm = ref({
  name: '',
  bio: '',
  location: '',
  website: ''
})

const startEditing = () => {
  editForm.value = {
    name: user.value.name,
    bio: user.value.bio || '',
    location: user.value.location || '',
    website: user.value.website || ''
  }
  editing.value = true
}

const saveProfile = async () => {
  try {
    const response = await api.patch('/users/me', editForm.value)
    user.value = response.data
    editing.value = false
  } catch (e: any) {
    console.error(e)
    alert('Failed to update profile')
  }
}

watch(() => route.params.username, (newUsername) => {
  username.value = newUsername as string || 'me'
  fetchProfile()
})
</script>

<template>
  <div v-if="loading" class="p-4 text-center">Loading...</div>
  <div v-else-if="error" class="p-4 text-center text-red-500">{{ error }}</div>
  <div v-else>
    <!-- Header -->
    <div class="sticky top-0 bg-black/80 backdrop-blur-md border-b border-gray-800 p-2 z-10 flex items-center space-x-4">
      <router-link to="/" class="p-2 hover:bg-gray-800 rounded-full">
        ←
      </router-link>
      <div>
        <h1 class="text-xl font-bold">{{ user?.name || username }}</h1>
        <p class="text-xs text-gray-500">0 posts</p>
      </div>
    </div>

    <!-- Cover & Avatar -->
    <div class="h-48 bg-gray-800"></div>
    <div class="px-4 relative mb-4">
      <div class="w-32 h-32 bg-gray-700 rounded-full border-4 border-black absolute -top-16"></div>
      <div class="flex justify-end py-4">
        <button 
          v-if="user?.username === JSON.parse(localStorage.getItem('user') || '{}').username"
          @click="startEditing"
          class="border border-gray-500 font-bold px-4 py-2 rounded-full hover:bg-gray-900"
        >
          Edit profile
        </button>
      </div>
    </div>

    <!-- Edit Modal -->
    <div v-if="editing" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div class="bg-black border border-gray-700 p-6 rounded-xl w-full max-w-md">
        <h2 class="text-xl font-bold mb-4">Edit Profile</h2>
        <form @submit.prevent="saveProfile" class="space-y-4">
          <div>
            <label class="block text-gray-500 text-sm mb-1">Name</label>
            <input v-model="editForm.name" type="text" class="w-full bg-black border border-gray-700 rounded p-2 focus:border-blue-500 outline-none">
          </div>
          <div>
            <label class="block text-gray-500 text-sm mb-1">Bio</label>
            <textarea v-model="editForm.bio" class="w-full bg-black border border-gray-700 rounded p-2 focus:border-blue-500 outline-none"></textarea>
          </div>
          <div class="flex justify-end space-x-4 mt-6">
            <button type="button" @click="editing = false" class="px-4 py-2 rounded-full hover:bg-gray-900">Cancel</button>
            <button type="submit" class="bg-white text-black font-bold px-4 py-2 rounded-full hover:bg-gray-200">Save</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Info -->
    <div class="px-4 mb-4">
      <h2 class="text-xl font-bold">{{ user?.name }}</h2>
      <p class="text-gray-500">@{{ user?.username }}</p>
      <p class="mt-4">{{ user?.bio || 'No bio yet.' }}</p>
      <div class="flex space-x-4 mt-2 text-gray-500">
        <span><strong class="text-white">100</strong> Following</span>
        <span><strong class="text-white">50</strong> Followers</span>
      </div>
    </div>

    <!-- Tabs -->
    <div class="flex border-b border-gray-800">
      <div class="flex-1 text-center py-4 hover:bg-gray-900 cursor-pointer border-b-4 border-blue-500 font-bold">
        Posts
      </div>
      <div class="flex-1 text-center py-4 hover:bg-gray-900 cursor-pointer text-gray-500">
        Replies
      </div>
      <div class="flex-1 text-center py-4 hover:bg-gray-900 cursor-pointer text-gray-500">
        Likes
      </div>
    </div>
  </div>
</template>
