import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'
import { GlobalContext } from './GlobalContext/GlobalContext';

export class FetchData extends Component {
  static displayName = FetchData.name;
  static contextType = GlobalContext;
  constructor(props) {
    super(props);
    this.state = { forecasts: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
    //this.createTask();
  // console.log('mounted');

   //console.log(this.context.user);
  }

  static renderForecastsTable(forecasts) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Title</th>
            <th>Date</th>
            <th>Creator</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map(forecast =>
            <tr key={forecast.Id}>
              <td>{forecast.Name}</td>
              <td>{forecast.CreatedDate}</td>
              <td>{forecast.Creator ? forecast.Creator.FirstName : 0}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderForecastsTable(this.state.forecasts);

    return (
      <div>
        <h1 id="tabelLabel" >List current tasks for user</h1>
        <p>This component demonstrates fetching data from the server.</p>
        {contents}
      </div>
    );
  }

  async populateWeatherData() {
    const token = await authService.getAccessToken();
    const response = await fetch('tasks/Index', {
      headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();
    console.log(data);
    console.log('user', this.context.user);
    this.setState({ forecasts: data, loading: false });
 //   console.log(data);
  }

  async createTask() {
    const token = await authService.getAccessToken();
     fetch('tasks/Create', {
      method: 'POST',
      body: JSON.stringify({
      Title: 'testing tasks',
      CreatedDate: '2023-04-05'}),
      headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Content-type': 'application/json; charset=UTF-8', }
    }).catch((err) => {
      console.log(err.message);
   });
    //const data = await response.json();
    //this.setState({ forecasts: data, loading: false });
  }
}
