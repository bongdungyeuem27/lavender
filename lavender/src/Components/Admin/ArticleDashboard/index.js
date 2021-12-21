import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import * as articleApi from "../../apis/article";
import LoadingContainer from "../../../Common/helper/loading/LoadingContainer";
import { stubTrue } from "lodash";
import { CLIENT_ENDPOINT } from "../../../Common/constants/index";
function ArticleDashboard() {
    const [posts, setPosts] = useState([]);
    const [verified, setVerified] = useState('duyệt');
    const [denied, setDenied] = useState('hủy yêu cầu');
    const [loading, setLoading] = useState(stubTrue);
    useEffect(() => {
        (async () => {
            await articleApi
                .allArticlePending()
                .then((res) => {
                    console.log(res.data);
                    // if(res.data.length > 0) {
                    //     setIsEmpty(false);
                    // }
                    setPosts(res.data);
                })
                .catch((err) => {
                    console.log(err);
                });
            setLoading(false);
        })();
    }, []);


    function verify(mabaiviet){
        articleApi.vertifyArticle(mabaiviet)
        .then((res) => {
            // setVerified('đã duyệt')
        })
    }


    function deny(mabaiviet){
        articleApi.deleteArticle(mabaiviet)
        .then((res) => {
            setDenied('đã hủy')
        })
    }
    return (
        <div>
            <LoadingContainer loading={loading}></LoadingContainer>
            <section>
                <div id="wrapper">
                    <div className="container">
                        <div className="row">
                            <div className="col-1"></div>
                            <div className="col-10">
                                <div className="page-wrapper">
                                    <div className="blog-top clearfix mb-5">
                                        <h3>Bài viết cần phê duyệt</h3>
                                    </div>
                                    {posts.map((post) => (
                                        <div className="blog-box pt-3 pb-2 ">
                                            <div>
                                                <div className=" row mb-4">
                                                    <div className="col-md-4">
                                                        <div className="post-media">
                                                            <img
                                                                src={post.thumnail}
                                                                alt=""
                                                                className="img-fluid"
                                                            />
                                                        </div>
                                                    </div>
                                                    <div className="blog-meta big-meta col-8">
                                                        <h5>
                                                            {post.tieude}
                                                        </h5>
                                                        <p>{post.mota}</p>
                                                        <input id={post.mabaiviet} 
                                                        className="btn btn-success"
                                                        type="button"
                                                         onClick = {()=>verify(post.mabaiviet)}
                                                         value ={verified}/>
                                                        <input id={post.mabaiviet}
                                                         className="btn btn-primary"
                                                        type="button" 
                                                        onClick = {()=>deny(post.mabaiviet)}
                                                        value={denied}/>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    );
}

export default ArticleDashboard;