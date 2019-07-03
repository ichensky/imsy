class Api {
    static ImageThumbUrl(id, s) { return "https://drive.google.com/thumbnail?id=" + id + "&sz=" + s; }
  
    async get(url) {
        var resp = await fetch(url);
        var json = await resp.json();
        if (json.errorcode === 0) {
            return json;
        }
    }
    async post(url, data) {
        var resp = await fetch(url, {
            method: "POST", body: JSON.stringify(data),

            headers: {
                "Content-Type": "application/json"
            }
        });
        var json = await resp.json();
        if (json.errorcode === 0) {
            return json;
        }
    }
    
    
    async GalleryImages(page=0) {
        var query = location.pathname.slice("/gallery/".length);
        query = decodeURI(query);
        return await this.post("../api/Gallery/Images", {query:query, page:page});
    }
}

class String{
    static IsEmptyOrWhiteSpaces(str) { return str === null || str.match(/^ *$/) !== null; }
}
class Threading {
    static Sleep(ms) { return new Promise(resolve => setTimeout(resolve, ms)); }
}