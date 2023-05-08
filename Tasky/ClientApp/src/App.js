import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import AuthorizeRoute from './components/api-authorization/AuthorizeRoute';
import { Layout } from './components/Layout';
import { GlobalContextProvider, GlobalContextData, SetUser } from './components/GlobalContext/GlobalContext.tsx';
import './custom.css';
import { library } from '@fortawesome/fontawesome-svg-core'
import { fab } from '@fortawesome/free-brands-svg-icons'
import { fas } from '@fortawesome/free-solid-svg-icons'
import { far } from '@fortawesome/free-regular-svg-icons'

library.add(fab, fas,far);

export default class App extends Component {
  static displayName = App.name;
  render() {
    return (
      <GlobalContextProvider>
        <Layout>
          <Routes>
            {AppRoutes.map((route, index) => {
              const { element, requireAuth, ...rest } = route;
              return <Route key={index} {...rest} element={requireAuth ? <AuthorizeRoute {...rest} element={element} /> : element} />;
            })}
          </Routes>
        </Layout>
      </GlobalContextProvider>
    );
  }
}
