import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { Subscription } from 'rxjs/Subscription';

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
    private sub: Subscription;

    constructor(
        private _route: ActivatedRoute,
        private _router: Router,
        private _stockService: StockService) {
    }

    ngOnInit(): void {
        this.stockLabel = this._route.snapshot.params['id'];
        this.getStock(this.stockLabel);
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