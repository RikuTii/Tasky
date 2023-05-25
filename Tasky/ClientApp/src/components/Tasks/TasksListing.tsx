import React, { useEffect, useState, useContext, useCallback } from "react";
import authService from "../api-authorization/AuthorizeService";
import "react-toastify/dist/ReactToastify.css";
import { GlobalContext } from "../GlobalContext/GlobalContext";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Tasklist, Task } from "../../types/tasks.d";
import { Container, Stack } from "react-bootstrap";
import debounce from "lodash/debounce";

const TasksListing = ({}) => {
  const [currentTaskList, setCurrentTaskList] = useState<Tasklist>();
  const [tasks, setTasks] = useState<Array<Task>>();
  const { user } = useContext(GlobalContext);


  useEffect(() => {
    loadTaskLists();
  }, []);

  const loadTaskLists = async () => {
    const token = await authService.getAccessToken();
    const response = await fetch("tasks/Index", {
      headers: !token ? {} : { Authorization: `Bearer ${token}` },
    });
    const data = await response.json();
    setCurrentTaskList(data[0]);
  };

  useEffect(() => {
    setTasks(currentTaskList?.tasks);
    console.log("loaded tasks",currentTaskList?.id);
  }, [currentTaskList]);

  const createNewTask = () => {
    const id = tasks?.slice(-1)[0].id;
    let nextId = 0;
    if (id) {
      nextId = id + 1;
    }
    const newTask: Task = {
      id: nextId,
      title: "",
      createdDate: new Date().toISOString(),
      creator: user?.id,
      taskList: currentTaskList,
    };
    if (tasks) {
      const newTasks = [...tasks];
      newTasks.push(newTask);
      setTasks(newTasks);
    }
  };


  const delayedTaskUpdate = useCallback( 
    debounce((q: Task) => onTaskUpdated(q), 1000), []);

  const onTaskUpdated = async (task: Task) => {
    const token = await authService.getAccessToken();
    fetch("task/CreateOrUpdateTask", {
      method: "POST",
      body: JSON.stringify({
        title: task?.title,
        id: task?.id,
        status: task?.status,
        taskListId: task?.taskList?.id,
      }),
      headers: !token
        ? {}
        : {
            Authorization: `Bearer ${token}`,
            "Content-type": "application/json; charset=UTF-8",
          },
    })
      .then(() => {      
      })
      .catch((err) => {
        console.log(err.message);
      });
  }


  return (
    <Container>
      <h1>Current Tasks</h1>
      {tasks?.map((task: Task) => (
        <Stack key={task.id} direction="horizontal" gap={3}>
          <FontAwesomeIcon icon={["fas", "list"]} color="white" size="1x" />
          <input
            type="textarea"
            placeholder=""
            value={task.title}
            onChange={(event) => {
              const newTasks = [...tasks];
              const currentTask = newTasks.find((e) => e.id === task.id);
              if (currentTask) {
                currentTask.title = event.target.value;
                setTasks(newTasks);
                delayedTaskUpdate(currentTask);
              }
            }}
          />
          <div
            style={{
              width: 25,
              height: 25,
              border: "2px solid white",
              alignItems: "center",
              justifyContent: "center",
            }}
            onClick={() => {
              const newTasks = [...tasks];
              const currentTask = newTasks.find((e) => e.id === task.id);
              if (currentTask) {
                if(currentTask.status === 0) {
                  currentTask.status = 1;
                }
                else {
                  currentTask.status = 0;
                }
                setTasks(newTasks);
              }
            }}
          >
            {task.status === 1 && (
              <div style={{ marginLeft: 4 }}>
              <FontAwesomeIcon
                icon={["fas", "check"]}
                color="white"
                size="1x"
              />
            </div>
            )}
    
          </div>
        </Stack>
      ))}
      <div style={{marginLeft: 'auto', alignItems: 'flex-end'}} onClick={createNewTask}>
        <FontAwesomeIcon
          icon={["fas", "circle-plus"]}
          color="white"
          size="2x"
        />
      </div>
    </Container>
  );
};

export default TasksListing;
