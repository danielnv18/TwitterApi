<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import api from '../services/api'

const router = useRouter()
const name = ref('')
const email = ref('')
const username = ref('')
const password = ref('')
const error = ref('')

const handleSignup = async () => {
  try {
    error.value = ''
    const response = await api.post('/auth/register', {
      name: name.value,
      email: email.value,
      username: username.value,
      password: password.value
    })
    
    // Auto login after signup or redirect to login
    // Assuming register returns token as well, if not redirect to login
    if (response.data.token) {
        localStorage.setItem('token', response.data.token)
        localStorage.setItem('user', JSON.stringify(response.data.user))
        router.push('/')
    } else {
        router.push('/login')
    }

  } catch (e: any) {
    console.error(e)
    if (e.response?.data?.errors) {
      // Format validation errors
      const errors = e.response.data.errors
      const errorMessages = Object.values(errors).flat().join('\n')
      error.value = errorMessages
    } else {
      error.value = e.response?.data?.message || 'Signup failed'
    }
  }
}
</script>

<template>
  <div>
    <h1 class="text-3xl font-bold mb-8">Create your account</h1>
    
    <div v-if="error" class="bg-red-500/10 border border-red-500 text-red-500 p-4 rounded mb-4 whitespace-pre-wrap">
      {{ error }}
    </div>

    <form @submit.prevent="handleSignup" class="space-y-4">
      <input 
        v-model="name"
        type="text" 
        placeholder="Name" 
        class="w-full bg-black border border-gray-700 rounded p-4 focus:border-blue-500 focus:outline-none"
      >
      <input 
        v-model="email"
        type="email" 
        placeholder="Email" 
        class="w-full bg-black border border-gray-700 rounded p-4 focus:border-blue-500 focus:outline-none"
      >
      <input 
        v-model="username"
        type="text" 
        placeholder="Username" 
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
        class="w-full bg-blue-500 text-white font-bold py-3 rounded-full hover:bg-blue-600 transition"
      >
        Sign up
      </button>
    </form>

    <div class="mt-8 text-gray-500">
      Have an account already? 
      <RouterLink to="/login" class="text-blue-500 hover:underline">Log in</RouterLink>
    </div>
  </div>
</template>
