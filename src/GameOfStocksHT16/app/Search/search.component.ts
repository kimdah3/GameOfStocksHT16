import { Component, OnInit } from '@angular/core';
import { StockService } from "../Stocks/stock.service";

import { IStock } from '../Stocks/stock';

@Component({
    selector: 'navsearch',
    templateUrl: 'templates/search.component.html',
    styleUrls: ['css/search.component.css'],
    providers: [ StockService ]
})
export class SearchComponent implements OnInit {
    stocks: IStock[];
    searchFilter: string;
    errorMessage: string;

    constructor(private _stockService: StockService) {
    }

    ngOnInit(): void {
        this._stockService.getStocks().subscribe(stocks => this.stocks = stocks,
        error => this.errorMessage = <any>error);
    }

    clearSearch(): void {
        this.searchFilter = "";
    }

    colorSelector(value: string): boolean {
        if (value.charAt(0) == "+") {
            return true;
        }
    }
}