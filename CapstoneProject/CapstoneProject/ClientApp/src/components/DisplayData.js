import React, { Component } from "react";
import Chart from "react-google-charts";
import { css } from "@emotion/react";
import ClipLoader from "react-spinners/ClipLoader";
import Tabs from 'react-bootstrap/Tabs';
import Tab from 'react-bootstrap/Tab';

export class DisplayData extends Component {
  constructor(props) {
    super(props);
    this.state = {
      incomingData: [],
      isLoading: true,
      voided: 0,
      paid: 0,
      draft: 0,
      authorised: 0,
      averageTotal: 0,
      uq: 0,
      lq: 0,
      highest: 0,
      lowest: 0,
      key: 'home',
    };
  }
  componentDidMount() {
    this.populateData();
  }
  async populateData() {
    const response = await fetch("test");
    const data = await response.json();
    this.setState({
      incomingData: data,
    });
    console.log(this.state.incomingData);
    this.calculateAveragePay(data);
    this.calculateTotalTransactionStatus(data);
    this.setState({ isLoading: false })
  }
  calculateTotalTransactionStatus(incoming) {
    incoming.forEach(item => {
      if (item.Status == "VOIDED") {
        this.setState({ voided: this.state.voided + 1 })
      }
      if (item.Status == "PAID") {
        this.setState({ paid: this.state.paid + 1 })
      }
      if (item.Status == "DRAFT") {
        this.setState({ draft: this.state.draft + 1 })
      }
      if (item.Status == "AUTHORISED") {
        this.setState({ authorised: this.state.authorised + 1 })
      }
    });
  }
  calculateAveragePay(incoming) {
    var totals = []
    var quarter = incoming.length / 4
    incoming.forEach(item => {
      totals.push(item.Total)
      this.setState({ averageTotal: this.state.averageTotal + item.Total })
    })
    totals.sort()
    this.setState({ averageTotal: this.state.averageTotal / incoming.length, uq: parseInt(totals[parseInt(quarter) * 3]), lq: parseInt(totals[parseInt(quarter)]), highest: parseInt(Math.max.apply(null, totals)), lowest: parseInt(Math.min.apply(null, totals)) })
    console.log(this.state.lowest)
    console.log(this.state.lq)
    console.log(this.state.uq)
    console.log(this.state.highest)
  }

  render() {
    var override = css`  
  border-color: red;
position: absolute;
left: 46%;
top: 43%;
transform: translate(-50%, -50%);
border: 5px solid #FFFF00;
`;
    return (
      <div className="xero-background">
        <h1 className="graph_display">Account statistics</h1>

        <ClipLoader color={"#ffffff"} loading={this.state.isLoading} css={override} size={150} />

        {
          !this.state.isLoading ? <Tabs
            id="controlled-tab-example"
            activeKey={this.state.key}
            onSelect={(k) => this.setState({ key: k })}
            className="mb-3"
          ><Tab eventKey="home" title="Transaction Status"> <div className="graphs">
            <div className="graph_display">
              <Chart
                width={'100%'}
                height={"350px"}
                chartType="PieChart"
                loader={<div><ClipLoader color={"#ffffff"} loading={this.state.isLoading} css={override} size={150} /></div>}
                data={[
                  ["Title", "Number",],
                  ["Voided", this.state.voided],
                  ["Paid", this.state.paid],
                  ["Draft", this.state.draft],
                  ["Authorised", this.state.authorised],
                ]}
                options={{
                    title: "Transaction status",
                    fontSize: 20,
                    animation: {
                        startup: true,
                        easing: 'linear',
                        duration: 1500,
                    },
                }}
              /></div></div></Tab>

          <Tab eventKey="payment_range" title="Payment Range">
          <div className="graphs">
            <div className="graph_display">
              <Chart
                width={'100%'}
                height={350}
                chartType="CandlestickChart"
                loader={<div><ClipLoader color={"#ffffff"} loading={this.state.isLoading} css={override} size={150} /></div>}
                data={[
                  ['day', 'a', 'b', 'c', 'd'],
                  ['totals', this.state.lowest, this.state.lq, this.state.uq, this.state.highest],

                ]}
                options={{
                  title: "Quartiles of transactions",
                    legend: 'none',
                    fontSize: 20,
                    animation: {
                        startup: true,
                        easing: 'linear',
                        duration: 1500,
                    }
                }}
                rootProps={{ 'data-testid': '1' }}
              /></div></div></Tab></Tabs> : null
        }

      </div>
    );
  }
}
