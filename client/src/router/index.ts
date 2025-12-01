import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: () => import('../components/Layout.vue'),
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('../views/Home.vue'),
        },
        {
          path: 'profile/:username',
          name: 'profile',
          component: () => import('../views/Profile.vue'),
        },
      ],
    },
    {
      path: '/auth',
      component: () => import('../components/AuthLayout.vue'),
      children: [
        {
          path: '/login',
          name: 'login',
          component: () => import('../views/Login.vue'),
        },
        {
          path: '/signup',
          name: 'signup',
          component: () => import('../views/Signup.vue'),
        },
      ],
    },
  ],
})

export default router
