import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';

import { IStock } from './stock';

@Injectable()
export class StockService {

    private _stocksUrl = 'api/stocks';

    constructor(private _http: Http) {

    }

    getStocks(): Observable<IStock[]> {
        return this._http.get(this._stocksUrl)
            .map((response: Response) => <IStock[]>response.json())
            .catch(this.handleError);
    }
    
    getStock(id: string): Observable<IStock> {
        return this.getStocks()
            .map((stocks: IStock[]) => stocks.find(s => s.label === id))
            .catch(this.handleError);
    }

    private handleError(error: Response) {
        console.log(error);
        return Observable.throw(error.json().error || 'Server error');
    }
}