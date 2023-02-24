import * as signalR from '@microsoft/signalr';

export default {
    install: (app, options) => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(options.url)
            .build();

        connection.start().then(() => {
            console.log('SignalR Connected');
        }).catch((err) => {
            console.error(err);
        });

        app.config.globalProperties.$signalR = connection;
    },
};