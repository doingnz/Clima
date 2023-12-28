/* eslint-disable max-classes-per-file */
/* eslint-disable no-restricted-globals */
/* eslint-disable no-undef */
$(document).ready(() => {
  // if deployed to a site supporting SSL, use wss://
  const protocol = document.location.protocol.startsWith('https') ? 'wss://' : 'ws://';
  const webSocket = new WebSocket(protocol + location.host);

  // A class for holding the last N points of telemetry for a device
  class DeviceData {
    constructor(deviceId) {
      this.deviceId = deviceId;
      this.maxLen = 1000;
      this.timeData = new Array(this.maxLen);
      this.temperatureData = new Array(this.maxLen);
      this.humidityData = new Array(this.maxLen);
      this.pressureData = new Array(this.maxLen);
      this.totalmemoryData = new Array(this.maxLen);
      this.batteryvoltageData = new Array(this.maxLen);
      this.solarvoltageData = new Array(this.maxLen);
    }

    addData(time, temperature, humidity, pressure, totalmemory, batteryvoltage, solarvoltage) {
      this.timeData.push(time);
      this.temperatureData.push(temperature);
      this.humidityData.push(humidity || null);
      this.pressureData.push(pressure);
      this.totalmemoryData.push(totalmemory);
      this.batteryvoltageData.push(batteryvoltage);
      this.solarvoltageData.push(solarvoltage);

      if (this.timeData.length > this.maxLen) {
        this.timeData.shift();
        this.temperatureData.shift();
        this.humidityData.shift();
        this.pressureData.shift();
        this.totalmemoryData.shift();
        this.batteryvoltageData.shift();
        this.solarvoltageData.shift();
        }
    }
  }

  // All the devices in the list (those that have been sending telemetry)
  class TrackedDevices {
    constructor() {
      this.devices = [];
    }

    // Find a device based on its Id
    findDevice(deviceId) {
      for (let i = 0; i < this.devices.length; ++i) {
        if (this.devices[i].deviceId === deviceId) {
          return this.devices[i];
        }
      }

      return undefined;
    }

    getDevicesCount() {
      return this.devices.length;
    }
  }

  const trackedDevices = new TrackedDevices();

  // Define the chart axes
  const chartDataTemperature = {
    datasets: [{
        fill: false,
        label: 'Temperature',
        yAxisID: 'Temperature',
        borderColor: 'rgba(35, 171, 227, 1)',
        pointBoarderColor: 'rgba(35, 171, 227, 1)',
        backgroundColor: 'rgba(35, 171, 227, 1)',
        pointHoverBackgroundColor: 'rgba(35, 171, 227, 1)',
        pointHoverBorderColor: 'rgba(35, 171, 227, 1)',
        spanGaps: true,
      }]
  };
  const chartDataHumidity = {
    datasets: [{
        fill: false,
        label: 'Humidity',
        yAxisID: 'Humidity',
        borderColor: 'rgba(201, 219, 49, 1)',
        pointBoarderColor: 'rgba(201, 219, 49, 1)',
        backgroundColor: 'rgba(201, 219, 49, 1)',
        pointHoverBackgroundColor: 'rgba(201, 219, 49, 1)',
        pointHoverBorderColor: 'rgba(201, 219, 49, 1)',
        spanGaps: true,
      }]
  };
  const chartDataPressure = {
    datasets: [{
        fill: false,
        label: 'Pressure',
        yAxisID: 'Pressure',
        borderColor: 'rgba(239, 125, 59, 1)',
        pointBoarderColor: 'rgba(239, 125, 59, 1)',
        backgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBackgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBorderColor: 'rgba(239, 125, 59, 1)',
        spanGaps: true,
      }]
  };

  const chartDataTotalMemory = {
    datasets: [{
        fill: false,
        label: 'Memory',
        yAxisID: 'Memory',
        borderColor: 'rgba(239, 125, 59, 1)',
        pointBoarderColor: 'rgba(239, 125, 59, 1)',
        backgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBackgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBorderColor: 'rgba(239, 125, 59, 1)',
        spanGaps: true,
      }]
  };
  const chartDataBatteryVoltage = {
    datasets: [{
        fill: false,
        label: 'Battery',
        yAxisID: 'Battery',
        borderColor: 'rgba(239, 125, 59, 1)',
        pointBoarderColor: 'rgba(239, 125, 59, 1)',
        backgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBackgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBorderColor: 'rgba(239, 125, 59, 1)',
        spanGaps: true,
      }]
  };  const chartDataSolarVoltage = {
    datasets: [{
        fill: false,
        label: 'Solar',
        yAxisID: 'Solar',
        borderColor: 'rgba(239, 125, 59, 1)',
        pointBoarderColor: 'rgba(239, 125, 59, 1)',
        backgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBackgroundColor: 'rgba(239, 125, 59, 1)',
        pointHoverBorderColor: 'rgba(239, 125, 59, 1)',
        spanGaps: true,
      }]
  };


  const chartOptionsTemperature = {
    scales: {
      yAxes: [{
        id: 'Temperature',
        type: 'linear',
        scaleLabel: {
          labelString: 'Temperature (ºC)',
          display: true,
        },
        position: 'left',
      }]
    }
  };

  const chartOptionsHumidity = {
    scales: {
      yAxes: [{
        id: 'Humidity',
        type: 'linear',
        scaleLabel: {
          labelString: 'Humidity (%)',
          display: true,
        },
        position: 'left',
      }]
    }
  };

  const chartOptionsPressure = {
    scales: {
      yAxes: [{
        id: 'Pressure',
        type: 'linear',
        scaleLabel: {
          labelString: 'Pressure (mbar)',
          display: true,
        },
        position: 'left',
      }]
    }
  };

  const chartOptionsTotalMemory = {
    scales: {
      yAxes: [{
        id: 'Memory',
        type: 'linear',
        scaleLabel: {
          labelString: 'Memory (bytes)',
          display: true,
        },
        position: 'left',
      }]
    }
  };
  const chartOptionsBatteryVoltage = {
    scales: {
      yAxes: [{
        id: 'Battery',
        type: 'linear',
        scaleLabel: {
          labelString: 'Battery (volts)',
          display: true,
        },
        position: 'left',
      }]
    }
  };
  const chartOptionsSolarVoltage = {
    scales: {
      yAxes: [{
        id: 'Solar',
        type: 'linear',
        scaleLabel: {
          labelString: 'Solar (volts)',
          display: true,
        },
        position: 'left',
      }]
    }
  };

  // Get the context of the canvas element we want to select
  const ctxTemperature = document.getElementById('iotTemperature').getContext('2d');
  const lineChartTemperature = new Chart(
    ctxTemperature,
    {
      type: 'line',
      data: chartDataTemperature,
      options: chartOptionsTemperature,
    });

  const ctxHumidity = document.getElementById('iotHumidity').getContext('2d');
  const lineChartHumidity = new Chart(
    ctxHumidity,
    {
      type: 'line',
      data: chartDataHumidity,
      options: chartOptionsHumidity,
    });

  const ctxPressure = document.getElementById('iotPressure').getContext('2d');
  const lineChartPressure = new Chart(
    ctxPressure,
    {
      type: 'line',
      data: chartDataPressure,
      options: chartOptionsPressure,
    });

  const ctxMemory = document.getElementById('iotMemory').getContext('2d');
  const lineChartTotalMemory = new Chart(
    ctxMemory,
    {
      type: 'line',
      data: chartDataTotalMemory,
      options: chartOptionsTotalMemory,
    });

  const ctxBattery = document.getElementById('iotBattery').getContext('2d');
  const lineChartBatteryVoltage = new Chart(
    ctxBattery,
    {
      type: 'line',
      data: chartDataBatteryVoltage,
      options: chartOptionsBatteryVoltage,
    });

  const ctxSolar = document.getElementById('iotSolar').getContext('2d');
  const lineChartSolarVoltage = new Chart(
    ctxSolar,
    {
      type: 'line',
      data: chartDataSolarVoltage,
      options: chartOptionsSolarVoltage,
    });
          


  // Manage a list of devices in the UI, and update which device data the chart is showing
  // based on selection
  let needsAutoSelect = true;
  const deviceCount = document.getElementById('deviceCount');
  const listOfDevices = document.getElementById('listOfDevices');
  function OnSelectionChange() {
    const device = trackedDevices.findDevice(listOfDevices[listOfDevices.selectedIndex].text);
    chartDataTemperature.labels = device.timeData;
    chartDataTemperature.datasets[0].data = device.temperatureData;
    chartDataHumidity.labels = device.timeData;
    chartDataHumidity.datasets[0].data = device.humidityData;
    chartDataPressure.labels = device.timeData;
    chartDataPressure.datasets[0].data = device.pressureData;
    chartDataTotalMemory.labels = device.timeData;
    chartDataTotalMemory.datasets[0].data = device.totalmemoryData;
    chartDataBatteryVoltage.labels = device.timeData;
    chartDataBatteryVoltage.datasets[0].data = device.batteryvoltageData;
    chartDataSolarVoltage.labels = device.timeData;
    chartDataSolarVoltage.datasets[0].data = device.solarvoltageData;
    lineChartTemperature.update();
    lineChartHumidity.update();
    lineChartPressure.update();
    linechartTotalMemory.update();
    linechartBatteryVoltage.update();
    linechartSolarVoltage.update();
  }
  listOfDevices.addEventListener('change', OnSelectionChange, false);

  // When a web socket message arrives:
  // 1. Unpack it
  // 2. Validate it has date/time and temperature
  // 3. Find or create a cached device to hold the telemetry data
  // 4. Append the telemetry data
  // 5. Update the chart UI
  webSocket.onmessage = function onMessage(message) {
    try {
      const messageData = JSON.parse(message.data);
      console.log(messageData);

      // time and either temperature or humidity are required
      if (!messageData.MessageDate || 
        (
          !messageData.IotData.temperature && 
          !messageData.IotData.humidity && 
          !messageData.IotData.pressure && 
          !messageData.IotData.totalmemory && 
          !messageData.IotData.batteryvoltage &&
          !messageData.IotData.solarvoltage
          )) {
        return;
      }

      // find or add device to list of tracked devices
      const existingDeviceData = trackedDevices.findDevice(messageData.DeviceId);

      if (existingDeviceData) {
        existingDeviceData.addData(
          messageData.MessageDate, 
          messageData.IotData.temperature, 
          messageData.IotData.humidity, 
          messageData.IotData.pressure,
          messageData.IotData.totalmemory,
          messageData.IotData.batteryvoltage,
          messageData.IotData.solarvoltage
          );
      } else {
        const newDeviceData = new DeviceData(messageData.DeviceId);
        trackedDevices.devices.push(newDeviceData);
        const numDevices = trackedDevices.getDevicesCount();
        deviceCount.innerText = numDevices === 1 ? `${numDevices} Meadow device` : `${numDevices} Meadow devices`;
        newDeviceData.addData(
          messageData.MessageDate, 
          messageData.IotData.temperature, 
          messageData.IotData.humidity, 
          messageData.IotData.pressure,
          messageData.IotData.totalmemory,
          messageData.IotData.batteryvoltage,
          messageData.IotData.solarvoltage);

        // add device to the UI list
        const node = document.createElement('option');
        const nodeText = document.createTextNode(messageData.DeviceId);
        node.appendChild(nodeText);
        listOfDevices.appendChild(node);

        // if this is the first device being discovered, auto-select it
        if (needsAutoSelect) {
          needsAutoSelect = false;
          listOfDevices.selectedIndex = 0;
          OnSelectionChange();
        }
      }

      lineChartTemperature.update();
      lineChartHumidity.update();
      lineChartPressure.update();
      lineChartTotalMemory.update();
      lineChartBatteryVoltage.update();
      lineChartSolarVoltage.update();
    } catch (err) {
      console.error(err);
    }
  };
});
