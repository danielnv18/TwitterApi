<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import api from '../services/api'

const router = useRouter()
const email = ref('')
const password = ref('')
const error = ref('')

const handleLogin = async () => {
  try {
    error.value = ''
    const response = await api.post('/auth/login', {
      email: email.value,
      password: password.value
    })
    
    localStorage.setItem('token', response.data.token)
    localStorage.setItem('user', JSON.stringify(response.data.user))
    
    router.push('/')
  } catch (e: any) {
    console.error(e)
    error.value = e.response?.data?.detail || 'Login failed'
  }
}
</script>

<template>
  <div>
    <h1 class="text-3xl font-bold mb-8">Sign in to X</h1>
    
    <form @submit.prevent="handleLogin" class="space-y-4">
      <input 
        v-model="email"
        type="email" 
        placeholder="Email" 
        class="w-full bg-black border border-gray-700 rounded p-4 focus:border-blue-500 focus:outline-none"
      >
      <input 
        v-model="password"
        type="password" 
        placeholder="Password" 
        class="w-full bg-black border border-gray-700 rounded p-4 focus:border-blue-500 focus:outline-none"
      >
      
      <button 
        type="submit"
        class="w-full bg-white text-black font-bold py-3 rounded-full hover:bg-gray-200 transition"
      >
        Next
      </button>
    </form>

    <div class="mt-8 text-gray-500">
      Don't have an account? 
      <RouterLink to="/signup" class="text-blue-500 hover:underline">Sign up</RouterLink>
    </div>
  </div>
</template>
