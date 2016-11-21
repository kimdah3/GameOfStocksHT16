import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule } from '@angular/http';

import { routing } from "./app.routing";

import { AppComponent } from './app.component';
import { WelcomeComponent } from "./welcome/welcome.component";
import { StockListComponent } from "./Stocks/stock-list.component";

@NgModule({
    imports: [
        BrowserModule,
        routing,
        HttpModule
    ],
    declarations: [AppComponent, WelcomeComponent, StockListComponent],
    bootstrap: [AppComponent]
})

export class AppModule { }