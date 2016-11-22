import { PipeTransform, Pipe } from '@angular/core';

import { IStock } from './stock';

@Pipe({
    name: 'stockFilter'
})

export class StockFilterPipe implements PipeTransform {

    transform(value: IStock[], filterBy: string): IStock[] {
        filterBy = filterBy ? filterBy.toLocaleLowerCase() : null;
        return filterBy ? value.filter((stock: IStock) =>
            stock.name.toLocaleLowerCase().indexOf(filterBy) !== -1) : value;

    }
}