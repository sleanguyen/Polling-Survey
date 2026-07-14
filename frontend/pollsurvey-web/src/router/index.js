import { createRouter, createWebHistory } from 'vue-router'
import HomeView       from '@/views/HomeView.vue'
import CreatePollView from '@/views/CreatePollView.vue'
import PollView       from '@/views/PollView.vue'
import ResultsView    from '@/views/ResultsView.vue'
import AnalyticsView  from '@/views/AnalyticsView.vue'
import LoginView      from '@/views/LoginView.vue'

const routes = [
  { path: '/',                name: 'Home',      component: HomeView       },
  { path: '/login',           name: 'Login',     component: LoginView      },
  { path: '/create',          name: 'Create',    component: CreatePollView },
  { path: '/poll/:code',      name: 'Poll',      component: PollView       },
  { path: '/results/:code',   name: 'Results',   component: ResultsView    },
  { path: '/analytics/:code', name: 'Analytics', component: AnalyticsView  }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
