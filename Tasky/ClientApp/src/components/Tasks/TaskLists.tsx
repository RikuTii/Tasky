import React, { useEffect, useState, useContext } from "react";
import authService from "../api-authorization/AuthorizeService";
import { ToastOptions, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { GlobalContext } from "../GlobalContext/GlobalContext";
import { Table } from "reactstrap";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
type Tasklist = {
  id: number;
  name: string;
  createdDate: string;
  creator: any;
  taskListMetas: any;
};

const TaskLists = ({}) => {
  const [tasklists, setTaskLists] = useState<Tasklist[]>();
  const [shareList, setShareList] = useState<Tasklist>();
  const [showShareModal, setShowShareModal] = useState(false);

  const { user, setUser } = useContext(GlobalContext);

  const toastProperties: ToastOptions = {
    position: "bottom-center",
    autoClose: 5000,
    hideProgressBar: false,
    closeOnClick: true,
    pauseOnHover: true,
    draggable: true,
    progress: 0,
    theme: "dark",
  };

  const loadTaskLists = async () => {
    const token = await authService.getAccessToken();
    const response = await fetch("tasks/Index", {
      headers: !token ? {} : { Authorization: `Bearer ${token}` },
    });
    const data = await response.json();
    setTaskLists(data);
    console.log(data);
  };

  const handleClose = () => setShowShareModal(false);
  const handleOpen = () => setShowShareModal(true);

  useEffect(() => {
    loadTaskLists();
  }, []);

  return (
    <div>
      <h1>Create new tasklist</h1>
      <Table striped bordered hover>
        <thead>
          <tr>
            <th>Title</th>
            <th>Date</th>
            <th>Creator</th>
            <th>Share</th>
            <th>Delete</th>
          </tr>
        </thead>
        <tbody>
          {tasklists?.map((tasklist) => (
            <tr key={tasklist.id}>
              <td>{tasklist.name}</td>
              <td>{tasklist.createdDate}</td>
              <td>{tasklist.creator ? tasklist.creator.firstName : 0}</td>
              <td>
                <div
                  style={{ alignContent: "center", justifyContent: "center" }}
                  onClick={() => {
                    setShareList(tasklist);
                    handleOpen();
                  }}
                >
                  {tasklist.creator && tasklist.creator.id == user?.id && (
                    <FontAwesomeIcon icon={["fas", "share-nodes"]} size="2xl" />
                  )}
                </div>
              </td>
              <td>
                <div
                  style={{ alignContent: "center", justifyContent: "center" }}
                >
                  {tasklist.creator && tasklist.creator.id == user?.id && (
                    <FontAwesomeIcon
                      icon={["fas", "trash"]}
                      color="red"
                      size="2xl"
                    />
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </Table>

      <Modal show={showShareModal} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Share tasklist</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <h3>Shared users</h3>
          <div>
            <p>{shareList?.name}</p>
            {shareList?.taskListMetas.map((meta: any) => {
              return <p>{meta.userAccount.firstName}</p>;
            })}
          </div>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleClose}>
            Close
          </Button>
          <Button variant="primary" onClick={handleClose}>
            Save Changes
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default TaskLists;
