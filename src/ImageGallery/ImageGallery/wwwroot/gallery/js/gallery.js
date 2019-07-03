class Magic {
    static get ImageSize() { return 480; }
    static get MagicNumber() { return 0.00375; }
    static get ImagesToLoad() {return 3;}
}

class DomInfo{

    static IsNeedLoadMore() {
        var documentSize = document.documentElement.scrollHeight;
        var shownDocumentSize = window.innerHeight + window.pageYOffset;
        return (shownDocumentSize + (Magic.ImageSize * Magic.ImagesToLoad)) - documentSize > 0; 
    }

    static LoadImage(img, url) {
        return new Promise(resolve => {
            img.onload = () => resolve();
            img.onerror  = () => resolve();
            img.src = url;
        });
    }

    static async ConvertToImageElements(images) {
        var imageEls = [];
        var tasks = [];

        for (var i = 0; i < images.length; i++) {
            var a = document.createElement("a");
            a.setAttribute('target', '_blank');

            var img = new Image();
            tasks.push(DomInfo.LoadImage(img, images[i].url));

            a.appendChild(img);
            a.href = "../image/" + images[i].strId;
            imageEls.push(a);
        }
        await Promise.all(tasks);
        return imageEls;
    }
   
}

class Dom{
    get GalleryElement() { return document.getElementById("gallery"); }
    PopulateGalleryWithColumns(columnsCount) {
        for (var j = 0; j < columnsCount; j++) {
            var column = document.createElement("div");
            column.className = "column";
            this.GalleryElement.appendChild(column);
        }
    }
    PopulateGalleryColumnsWithImages(imageElements) {
        var columns = this.GalleryElement.children;
        for (var i = 0; i < imageElements.length; i += columns.length) {
            var left = imageElements.length - i;
            var length = columns.length < left ? columns.length : left;
            for (var j = 0; j < length; j++) {
                columns[j].appendChild(imageElements[i + j]);
            }
        }
    }
    RemoveGalleryColumnsWithImages () {
        var imageElements = [];
        while (this.GalleryElement.firstElementChild) {
            var ims = this.GalleryElement.firstElementChild.children;
            imageElements.push.apply(imageElements, ims);
            this.GalleryElement.removeChild(this.GalleryElement.firstElementChild);
        }
        return imageElements;
    }
    IsNeededLoadMoreImages () {
        var columns = this.GalleryElement.children;
        for (var i = 0; i < columns.length; i++) {
            var column = columns[i];
            var columnHeight = column.clientHeight;
            var images = column.children;
            var imagesHeight = 0;
            for (var j = 0; j < images.length; j++) {
                imagesHeight += images[i].clientHeight;
            }
            if (imagesHeight < columnHeight - 10) {
                return true;
            }
        }
        return false;
    }
}

class DomState{
    constructor() {
        this.PagesCount = 0;
        this.GalleryImagesColumnsCount = 0;
    }
    UpdateGalleryImagesColumnsCount () {
        var col = Math.floor(window.innerWidth * Magic.MagicNumber);
        if (col < 1) {
            col = 1;
        }
        this.GalleryImagesColumnsCount = col;
    }
}

class Gallery{
    constructor(api,dom, domState) {
        this._api = api;
        this._dom = dom;
        this._domState = domState;
        this._loading = false;
        this._canLoadMore = true;
    }

    async LoadImages(page, isNewQuery) {
        var resp = await this._api.GalleryImages(page);
        var images = resp.images;
        if (images.length===0) {
            this._canLoadMore = false;
        }
        var imageElements = await DomInfo.ConvertToImageElements(images);
        if (isNewQuery) {
            this._dom.RemoveGalleryColumnsWithImages();
            this._dom.PopulateGalleryWithColumns(this._domState.GalleryImagesColumnsCount);
        }
        this._dom.PopulateGalleryColumnsWithImages(imageElements);
    }

    async LoadMoreImagesIfNeeded() {
        if (this._loading || !this._canLoadMore) {
            return;
        }
        this._loading = true;
        while (DomInfo.IsNeedLoadMore()) {
            await this.LoadImages(this._domState.PagesCount, this._domState.PagesCount === 0);
            this._domState.PagesCount++;
        }
        this._loading = false;
    }
}

var dom = new Dom(); 
var domState = new DomState();
var api = new Api();
var gallery = new Gallery(api, dom, domState);
domState.UpdateGalleryImagesColumnsCount();
gallery.LoadMoreImagesIfNeeded();

let onWindowResizeEventHandle = () => {
    window.onresize = async () => {
        var columnsCount = domState.GalleryImagesColumnsCount;
        domState.UpdateGalleryImagesColumnsCount();
        if (columnsCount !== domState.GalleryImagesColumnsCount) {
            var imageElements = dom.RemoveGalleryColumnsWithImages();
            dom.PopulateGalleryWithColumns(domState.GalleryImagesColumnsCount);
            dom.PopulateGalleryColumnsWithImages(imageElements);
        }
        
        gallery.LoadMoreImagesIfNeeded();
    };
};
let onScrollEventHandle = () => {
    window.onscroll = async () => {
        gallery.LoadMoreImagesIfNeeded();
    };
};
onWindowResizeEventHandle();
onScrollEventHandle();

//var searchinputElement = document.getElementById("searchinput");
//searchinputElement.onsubmit = function (e) {
//    e.stopPropagation();
//    gallery.LoadMoreImagesIfNeeded();
//}