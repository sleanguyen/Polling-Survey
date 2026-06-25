import { createRouter, createWebHistory } from 'vue-router'
import HomeView       from '@/views/HomeView.vue'
import CreatePollView from '@/views/CreatePollView.vue'
import PollView       from '@/views/PollView.vue'
import ResultsView    from '@/views/ResultsView.vue'

const routes = [
  { path: '/',              name: 'Home',    component: HomeView       },
  { path: '/create',        name: 'Create',  component: CreatePollView },
  { path: '/poll/:code',    name: 'Poll',    component: PollView       },
  { path: '/results/:code', name: 'Results', component: ResultsView    }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
