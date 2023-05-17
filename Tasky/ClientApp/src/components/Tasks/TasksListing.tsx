import React, { useEffect, useState, useContext } from "react";
import authService from "../api-authorization/AuthorizeService";
import { ToastOptions, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { GlobalContext } from "../GlobalContext/GlobalContext";
import { Table } from "reactstrap";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Tasklist, Task } from "../../types/tasks.d";

const TasksListing = ({}) => {
  const [tasklists, setTaskLists] = useState<Tasklist[]>();
  const [tasks, setTasks] = useState<Task[]>();
  const { user, setUser } = useContext(GlobalContext);

  const loadTaskLists = async () => {
    const token = await authService.getAccessToken();
    const response = await fetch("tasks/Index", {
      headers: !token ? {} : { Authorization: `Bearer ${token}` },
    });
    const data = await response.json();
    setTaskLists(data);
    setTasks(data[0].tasks);
  };

  useEffect(() => {
    loadTaskLists();
  }, []);

  return (
    <div>
      <h1>Current Tasks</h1>

      {tasks?.map((task: Task) => (
        <div>{task.title}</div>
      ))}
    </div>
  );
};

export default TasksListing;
