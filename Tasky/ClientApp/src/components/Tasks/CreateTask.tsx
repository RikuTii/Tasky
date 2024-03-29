import React, { useEffect, useState, useContext } from "react";
import authService from "../api-authorization/AuthorizeService";
import Button from "react-bootstrap/Button";
import Form from "react-bootstrap/Form";
import { ToastOptions, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { GlobalContext } from "../GlobalContext/GlobalContext";
import { toastProperties } from "../../types/global.d";


const CreateTask = ({}) => {
  const [tasklist, setTaskList] = useState();
  const [newTask, setNewTask] = useState({ name: "", description: "", listId: 0 });
  const { user, setUser } = useContext(GlobalContext);

  useEffect(() => {

  }, []);

  const loadTaskLists = async () => {
    const token = await authService.getAccessToken();
    const response = await fetch("tasks/TaskList", {
      headers: !token ? {} : { Authorization: `Bearer ${token}` },
    });
    const data = await response.json();
    console.log(data);
    //  this.setState({ tasklist: data.Result, loading: false });
  };

  const createNewTask = async () => {
    const token = await authService.getAccessToken();
    fetch("tasks/CreateTask", {
      method: "POST",
      body: JSON.stringify({
        Name: newTask?.name,
        Description: newTask?.description,
        Id: newTask?.listId,
      }),
      headers: !token
        ? {}
        : {
            Authorization: `Bearer ${token}`,
            "Content-type": "application/json; charset=UTF-8",
          },
    })
      .then(() => {
        toast("Created new task: " + newTask?.name, toastProperties);
        setNewTask({ name: "", description: "", listId: 0 });
      })
      .catch((err) => {
        console.log(err.message);
      });
  };

  return (
    <div>
      <h1>Create new tasklist</h1>
      <Form>
        <Form.Group className="mb-3" controlId="formName">
          <Form.Label>Name</Form.Label>
          <Form.Control
            type="text"
            placeholder="Name"
            value={newTask.name}
            onChange={(event) => {
              const data = {
                name: event.target.value,
                description: newTask.description,
                listId: newTask.listId
              };
              setNewTask(data);
            }}
          />
        </Form.Group>

        <Form.Group className="mb-3" controlId="formDescription">
          <Form.Label>Description</Form.Label>
          <Form.Control
            type="textarea"
            placeholder="Description"
            value={newTask.description}
            onChange={(event) => {
              const data = {
                name: newTask.name,
                description: event.target.value,
                listId: newTask.listId
              };
              setNewTask(data);
            }}
          />
        </Form.Group>
        <Button variant="primary" type="button" onClick={createNewTask}>
          Create new task
        </Button>
      </Form>
    </div>
  );
};

export default CreateTask;
