import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Redirect } from './components/Redirect';
import { Login } from './components/Login';
import axios from 'axios';
import './custom.css'
import { DisplayData } from './components/DisplayData';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
            <Route path='/fetch-data' component={FetchData} />            
            <Route path='/redirect' component={Redirect} />
            <Route path='/display' component={DisplayData}/>
      </Layout>
    );
  }
}
