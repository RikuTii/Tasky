import ApiAuthorzationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import CreateTask from "./components/Tasks/CreateTaskList";
import { Home } from "./components/Home";
import TaskLists from './components/Tasks/TaskLists';

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    requireAuth: true,
    element: <TaskLists />
  },
  {
    path: '/create-tasklist',
    requireAuth: true,
    element: <CreateTask />
  },
  ...ApiAuthorzationRoutes
];

export default AppRoutes;
