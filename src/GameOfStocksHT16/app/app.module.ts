﻿import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { routing } from "./app.routing";

import { AppComponent } from './app.component';
import { WelcomeComponent } from "./welcome/welcome.component";
import { StockListComponent } from "./Stocks/stock-list.component";
import { StockComponent } from "./Stocks/stock.component";
import { StockFilterPipe } from './Stocks/stock-filter.pipe';

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        routing,
        HttpModule
    ],
    declarations: [
        AppComponent,
        WelcomeComponent,
        StockListComponent,
        StockComponent,
        StockFilterPipe
    ],
    bootstrap: [AppComponent]
})

export class AppModule { }