export type Tasklist = {
    id?: number;
    name?: string;
    createdDate?: string;
    creator?: any;
    taskListMetas?: any;
    tasks?: Task[];
  };
  
export type Task = {
    id?: number;
    title?: string;
    createdDate?: string;
    creator?: any;
    taskList?: Tasklist;
    status?: number;
};