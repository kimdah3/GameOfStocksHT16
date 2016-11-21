import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { StockService } from "./stock.service";
import { IStock } from './stock';

@Component({
    templateUrl: 'templates/stock.component.html',
    providers: [StockService]
})

export class StockComponent implements OnInit {
    stock: IStock;
    stockLabel: string;
    errorMessage: string;

    constructor(
        private _stockService: StockService,
        private _route: ActivatedRoute,
        private _router: Router) {
    }

    ngOnInit(): void {
        //this.stockLabel = this._route.snapshot.params['id'];
        this.getStock("REJL-B.ST");
    }

    getStock(id: string) {
        this._stockService.getStock(id)
            .subscribe(
            stock => this.stock = stock,
            error => this.errorMessage = <any>error);
    }

    onBack(): void {
        this._router.navigate(['/stocks']);
    }
};