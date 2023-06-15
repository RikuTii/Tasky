import React, { useEffect, useState, useContext, useCallback } from "react";
import authService from "../api-authorization/AuthorizeService";
import "react-toastify/dist/ReactToastify.css";
import { GlobalContext } from "../GlobalContext/GlobalContext";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Tasklist, Task, TaskStatus } from "../../types/tasks.d";
import { Container, Stack } from "react-bootstrap";
import debounce from "lodash/debounce";
import Dropdown from "react-bootstrap/Dropdown";
import DropdownButton from "react-bootstrap/DropdownButton";
import { forEach } from "lodash";

const TasksListing = ({}) => {
  const [currentTaskList, setCurrentTaskList] = useState<Tasklist>();
  const [taskLists, setTaskLists] = useState<Tasklist[]>();
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
    setTaskLists(data);
  };

  const refreshTaskLists = async (id: number) => {
    const token = await authService.getAccessToken();
    const response = await fetch("tasks/Index", {
      headers: !token ? {} : { Authorization: `Bearer ${token}` },
    });
    const data = await response.json();
    setTaskLists(data);
    forEach(data, (taskList: Tasklist) => {
      if (taskList.id == id) {
        setCurrentTaskList(taskList);
      }
    });
  };

  useEffect(() => {
    setTasks(currentTaskList?.tasks);
  }, [currentTaskList]);

  const createNewTask = () => {
    let id = 1;
    if (tasks && tasks?.length > 0) {
      id = tasks?.slice(-1)[0].id ?? 1;
    }
    let nextId = id + 1;

    const newTask: Task = {
      id: nextId,
      title: "",
      createdDate: new Date().toISOString(),
      creator: user?.id,
      status: TaskStatus.NotCreated,
      taskList: currentTaskList,
      taskListID: currentTaskList?.id,
    };
    if (tasks) {
      const newTasks = [...tasks];
      newTasks.push(newTask);
      setTasks(newTasks);
    }
  };

  const delayedTaskUpdate = useCallback(
    debounce((q: Task) => onTaskUpdated(q), 1000),
    []
  );

  const onTaskUpdated = async (task: Task) => {
    const token = await authService.getAccessToken();
    fetch("task/CreateOrUpdateTask", {
      method: "POST",
      body: JSON.stringify({
        title: task?.title,
        id: task?.id,
        status: task?.status,
        taskListId: task?.taskListID ?? task?.taskList?.id,
      }),
      headers: !token
        ? {}
        : {
            Authorization: `Bearer ${token}`,
            "Content-type": "application/json; charset=UTF-8",
          },
    })
      .then(() => {
        refreshTaskLists(task?.taskListID ?? task?.taskList?.id ?? 0);
      })
      .catch((err) => {
        console.log(err.message);
      });
  };

  const doneTasks = tasks?.filter(e => e.status === TaskStatus.Done);
  const pendingTasks = tasks?.filter(e => e.status !== TaskStatus.Done);

  const filteredTasks = pendingTasks?.concat(doneTasks ?? []);


  return (
    <Container>
      <h1>Current Tasks</h1>
      <h2>{currentTaskList?.name}</h2>
      <DropdownButton
        id="dropdown-basic-button"
        title="Tasklists"
        style={{ marginBottom: 8 }}
      >
        {taskLists?.map((taskList: Tasklist) => (
          <Dropdown.Item
            key={taskList.id}
            onClick={() => {
              setCurrentTaskList(taskList);
            }}
          >
            {taskList.name}
          </Dropdown.Item>
        ))}
      </DropdownButton>

      {filteredTasks?.map((task: Task) => (
        <Stack key={task.id} direction="horizontal" gap={3}>
          <FontAwesomeIcon icon={["fas", "list"]} color="white" size="1x" />
          <input
            type="textarea"
            placeholder=""
            value={task.title}
            style={{textDecoration: task.status === TaskStatus.Done ? 'line-through' : ''}}
            onChange={(event) => {
              const newTasks = [...tasks ?? []];
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
              const newTasks = [...tasks ?? []];
              const currentTask = newTasks.find((e) => e.id === task.id);
              if (currentTask) {
                if (
                  !currentTask.status ||
                  currentTask.status === TaskStatus.NotDone
                ) {
                  currentTask.status = TaskStatus.Done;
                } else {
                  currentTask.status = TaskStatus.NotDone;
                }
                delayedTaskUpdate(currentTask);
                setTasks(newTasks);
              }
            }}
          >
            {task.status === TaskStatus.Done && (
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

      <div style={{}} onClick={createNewTask}>
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
