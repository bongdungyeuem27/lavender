import React, { useState } from "react";
import "reactjs-popup/dist/index.css";
import Modal from "react-modal";
import * as customerAccountApi from "../../apis/customeraccount";
import * as myToast from "../../../Common/helper/toastHelper";
import "./style.css";
import Cookies from "universal-cookie";

const cookie = new Cookies();

const customStyles = {
  content: {
    top: "40%",
    left: "50%",
    right: "auto",
    bottom: "auto",
    marginRight: "-50%",
    transform: "translate(-50%, -50%)",
  },
};

export default function AddModal(props) {
  const [makhachhang, setMakhachhang] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [progress, setProgress] = useState(0);

  function submitHandler() {
    var token = cookie.get("token");
    var refreshtoken = cookie.get("refreshtoken");
    const fd = new FormData();
    fd.append("makhachhang", makhachhang);
    fd.append("username", username);
    fd.append("password", password);

    customerAccountApi
      .addAccount(fd, setProgress, token, refreshtoken)
      .then((success) => {
        props.addFunction(success.data.value);
        props.closeModal();
      })
      .catch((error) => {
        myToast.toastError("Thêm mới thất bại");
        console.error(error);
      });
  }

  return (
    <Modal
      isOpen={props.showModal}
      onRequestClose={props.closeModal}
      style={customStyles}
      contentLabel="Example Modal"

    >
      <div class=" add-item-modal taikhoankhachhang" role="document">
        <div class="">
          <div class="modal-header">
            <h5 class="modal-title" id="exampleModalLongTitle">
              Thêm mới tài khoản khách hàng
            </h5>
          </div>

          <div className="form-main-add-edit">
            <div className="row mb-1">
              <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6">
                Mã khách hàng
              </div>
              <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6 ">
                <input
                  className="form-control border "
                  id="makhachhang"
                  placeholder=""
                  onChange={(e) => setMakhachhang(e.target.value)}
                  value={makhachhang}
                ></input>
              </div>
            </div>

            <div className="row mb-1">
              <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6">
                Username
              </div>
              <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6 ">
                <input
                  className="form-control border"
                  type="username"
                  placeholder=""
                  onChange={(e) => setUsername(e.target.value)}
                  value={username}
                ></input>
              </div>
            </div>

            <div className="row mb-1">
              <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6">
                Password
              </div>
              <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6 ">
                <input
                  className="form-control border"
                  id="password"
                  placeholder=""
                  onChange={(e) => setPassword(e.target.value)}
                  value={password}
                ></input>
              </div>
            </div>
          </div>

          <hr></hr>
          <div className="progress">
            <div
              className="progress-bar"
              role="progressbar"
              style={{ width: progress + "%" }}
            ></div>
          </div>

          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-secondary"
              data-dismiss="modal"
              onClick={props.closeModal}
            >
              Đóng
            </button>
            <button
              type="button"
              class="btn btn-primary"
              onClick={submitHandler}
            >
              Thêm
            </button>
          </div>
        </div>
      </div>
    </Modal>
  );
}
