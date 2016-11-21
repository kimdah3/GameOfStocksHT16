import { Component, OnInit } from '@angular/core';
import { StockService } from "./stock.service";

import { IStock } from './stock';

@Component({
    templateUrl: 'templates/stock-list.component.html',
    providers: [ StockService ]
})
export class StockListComponent implements OnInit {
    stocks: IStock[];
    errorMessage: string;
    constructor(private _stockService: StockService) {
    }

    ngOnInit(): void {
        this._stockService.getStocks().subscribe(stocks => this.stocks = stocks,
        error => this.errorMessage = <any>error);
    }
}